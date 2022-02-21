using Core.Interfaces;
using Core.Models;
using Equinor.ProCoSys.FamWebJob.Core.Mappers;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.TI.Common.Messaging;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Fam.Core.EventHubs.Contracts;
using Fam.Models.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;

namespace Core.Services;

public class FamFeederService : IFamFeederService
{
    private readonly CommonLibConfig _commonLibConfig;
    private readonly IEventHubProducerService _eventHubProducerService;
    private readonly FamFeederOptions _famFeederOptions;
    private readonly ILogger<FamFeederService> _logger;
    private readonly IPlantRepository _plantRepository;
    private readonly IFamEventRepository _repo;

    public FamFeederService(IEventHubProducerService eventHubProducerService, IFamEventRepository repo,
        IOptions<CommonLibConfig> commonLibConfig, IOptions<FamFeederOptions> famFeederOptions,
        IPlantRepository plantRepository, ILogger<FamFeederService> logger)
    {
        _eventHubProducerService = eventHubProducerService;
        _repo = repo;
        _plantRepository = plantRepository;
        _logger = logger;
        _commonLibConfig = commonLibConfig.Value;
        _famFeederOptions = famFeederOptions.Value;
    }

    public async Task RunFeeder(QueryParameters queryParameters)
    {
        var mapper = CreateCommonLibMapper();
        if (queryParameters.PcsTopic == PcsTopic.WorkOrderCutoff)
        {
            await WoCutoff(mapper, queryParameters.Plant);
            return;
        }

        var events = new List<FamEvent>();
        (PcsTopic, string) fields = new();
        switch (queryParameters.PcsTopic)
        {
            case PcsTopic.CommPkg:
                events = await _repo.GetCommPackages(queryParameters.Plant);
                fields = QueryMapping.CommPkg;
                break;
            case PcsTopic.Ipo:
                break;
            case PcsTopic.McPkg:
                events = await _repo.GetMcPackages(queryParameters.Plant);
                fields = QueryMapping.McPkg;
                break;
            case PcsTopic.Project:
                events = await _repo.GetProjects(queryParameters.Plant);
                fields = QueryMapping.Project;
                break;
            case PcsTopic.Responsible:
                break;
            case PcsTopic.Tag:
                events = await _repo.GetTags(queryParameters.Plant);
                fields = QueryMapping.Tag;
                break;
            case PcsTopic.TagFunction:
                break;
            case PcsTopic.PunchListItem:
                events = await _repo.GetPunchItems(queryParameters.Plant);
                fields = QueryMapping.PunchListItem;
                break;
            case PcsTopic.Library:
                break;
            case PcsTopic.WorkOrder:
                events = await _repo.GetWorkOrders(queryParameters.Plant);
                fields = QueryMapping.WorkOrder;
                break;
            case PcsTopic.Checklist:
                events = await _repo.GetCheckLists(queryParameters.Plant);
                fields = QueryMapping.CheckList;
                break;
            case PcsTopic.Milestone:
                events = await _repo.GetMilestones(queryParameters.Plant);
                fields = QueryMapping.Milesstone;
                break;
            case PcsTopic.WorkOrderCutoff:
                break;
            case PcsTopic.Certificate:
                break;
            case PcsTopic.WoChecklist:
                events = await _repo.GetWoChecklists(queryParameters.Plant);
                fields = QueryMapping.WorkOrderChecklist;
                break;
            default:
                return;
        }

        if (events.Count == 0 || string.IsNullOrEmpty(fields.Item2)) return;

        _logger.LogInformation(
            $"Found {events.Count} events for topic {queryParameters.PcsTopic} and plant {queryParameters.Plant}");

        var messageType = fields.Item1;
        var nameField = fields.Item2;

        var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e.Message, messageType, nameField));
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

        foreach (var batch in mappedMessages.Batch(250)) await SendFamMessages(batch);

        _logger.LogInformation($"Finished sending {queryParameters.PcsTopic} to fam");
    }

    public Task<List<string>> GetAllPlants()
    {
        return _plantRepository.GetAllPlants();
    }

    private SchemaMapper CreateCommonLibMapper()
    {
        ISchemaSource source = new ApiSource(new ApiSourceOptions
        {
            TokenProviderConnectionString = "RunAs=App;" +
                                            $"AppId={_commonLibConfig.ClientId};" +
                                            $"TenantId={_commonLibConfig.TenantId};" +
                                            $"AppKey={_commonLibConfig.ClientSecret}"
        });

        // Add caching functionality
        source = new CacheWrapper(
            source,
            maxCacheAge: TimeSpan.FromDays(1), // Use TimeSpan.Zero for no recache based on age
            checkForChangesFrequency: TimeSpan
                .FromHours(1)); // Use TimeSpan.Zero when cache should never check for changes.

        var mapper = new SchemaMapper("ProCoSys_Events", "FAM", source);
        return mapper;
    }

    private async Task WoCutoff(ISchemaMapper mapper, string plant)
    {
        var tasks = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" }.AsParallel()
            .Select(
                async i =>
                {
                    var connectionString = _famFeederOptions.ProCoSysConnectionString;
                    var response = await _repo.GetWoCutoffs(i, plant, connectionString);

                    var messages = response.SelectMany(e =>
                        TieMapper.CreateTieMessage(e.Message!, PcsTopic.WorkOrderCutoff, "WoNo"));
                    messages = messages.Select(m => mapper.Map(m).Message).ToList();

                    foreach (var batch in messages.Batch(250)) await SendFamMessages(batch);
                });
        await Task.WhenAll(tasks);
        _logger.LogInformation("Sent WoCutoff to FAM");
    }

    private async Task SendFamMessages(IEnumerable<Message> messages)
    {
        try
        {
            await _eventHubProducerService.SendDataAsync(messages);
        }
        catch (FamConfigException e)
        {
            throw new Exception("Configuration error: Could not send message.", e);
        }
        catch (Exception e)
        {
            throw new Exception("Error: Could not send message.", e);
        }
    }
}