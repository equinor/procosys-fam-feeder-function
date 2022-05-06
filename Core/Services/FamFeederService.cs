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
    private ILogger? _logger;
    private readonly IPlantRepository _plantRepository;
    private readonly IFamEventRepository _repo;

    public FamFeederService(IEventHubProducerService eventHubProducerService, IFamEventRepository repo,
        IOptions<CommonLibConfig> commonLibConfig, IOptions<FamFeederOptions> famFeederOptions,
        IPlantRepository plantRepository)
    {
        _eventHubProducerService = eventHubProducerService;
        _repo = repo;
        _plantRepository = plantRepository;
        _commonLibConfig = commonLibConfig.Value;
        _famFeederOptions = famFeederOptions.Value;
    }

    public async Task<string> RunFeeder(QueryParameters queryParameters, ILogger logger)
    {
        _logger = logger;
        var mapper = CreateCommonLibMapper();
        if (queryParameters.PcsTopic == PcsTopic.WorkOrderCutoff)
        {
            return "Cutoff Should have its own call";
        }

        var events = new List<FamEvent>();
        switch (queryParameters.PcsTopic)
        {
            case PcsTopic.CommPkg:
                events = await _repo.GetCommPackages(queryParameters.Plant);
                break;

            case PcsTopic.Ipo:
                break;

            case PcsTopic.McPkg:
                events = await _repo.GetMcPackages(queryParameters.Plant);
                break;

            case PcsTopic.Project:
                events = await _repo.GetProjects(queryParameters.Plant);
                break;

            case PcsTopic.Responsible:
                events = await _repo.GetResponsible(queryParameters.Plant);
                break;

            case PcsTopic.Tag:
                events = await _repo.GetTags(queryParameters.Plant);
                break;

            case PcsTopic.TagFunction:
                break;

            case PcsTopic.PunchListItem:
                events = await _repo.GetPunchItems(queryParameters.Plant);
                break;

            case PcsTopic.Library:
                events = await _repo.GetLibrary(queryParameters.Plant);
                break;

            case PcsTopic.WorkOrder:
                events = await _repo.GetWorkOrders(queryParameters.Plant);
                break;

            case PcsTopic.Checklist:
                events = await _repo.GetCheckLists(queryParameters.Plant);
                break;

            case PcsTopic.Milestone:
                events = await _repo.GetMilestones(queryParameters.Plant);
                break;

            case PcsTopic.WorkOrderCutoff:
                break;

            case PcsTopic.Certificate:
                break;

            case PcsTopic.WoChecklist:
                events = await _repo.GetWoChecklists(queryParameters.Plant);
                break;

            case PcsTopic.SWCR:
                events = await _repo.GetSwcr(queryParameters.Plant);
                break;

            case PcsTopic.SWCRSignature:
                events = await _repo.GetSwcrSignature(queryParameters.Plant);
                break;

            case PcsTopic.PipingRevision:
                events = await _repo.GetPipingRevision(queryParameters.Plant);
                break;
            case PcsTopic.PipingSpool:
                events = await _repo.GetPipingSpool(queryParameters.Plant);
                break;
            case PcsTopic.WoMaterial:
                events = await _repo.GetWoMaterials(queryParameters.Plant);
                break;

            case PcsTopic.Stock:
                events = await _repo.GetStock(queryParameters.Plant);
                break;

            case PcsTopic.WoMilestone:
                events = await _repo.GetWoMilestones(queryParameters.Plant);
                break;
            case PcsTopic.CommPkgOperation:
                events = await _repo.GetCommPkgOperations(queryParameters.Plant):
                break;

            case PcsTopic.Document:
                events = await _repo.GetDocument(queryParameters.Plant);
                break;

            default:
                {
                    _logger.LogInformation("Default switch statement, returning");
                    return "default error";
                }
        }

        if (events.Count == 0)
        {
            _logger.LogInformation("found no events, or field is null");
            return "found no events, or field is null";
        }

        _logger.LogInformation(
            $"Found {events.Count} events for topic {queryParameters.PcsTopic} and plant {queryParameters.Plant}");

        var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e.Message!, queryParameters.PcsTopic));
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

        foreach (var batch in mappedMessages.Batch(250)) await SendFamMessages(batch);

        _logger.LogInformation($"Finished sending {queryParameters.PcsTopic} to fam");

        return $"finished successfully sending {mappedMessages.Count} messages to fam for {queryParameters.PcsTopic}";
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

    public async Task<string> WoCutoff(string plant, string month, ILogger logger)
    {
        var mapper = CreateCommonLibMapper();
        var connectionString = _famFeederOptions.ProCoSysConnectionString;
        var response = await _repo.GetWoCutoffs(month, plant, connectionString);
        logger.LogInformation($"Found {response.Count} cutoffs for month {month} in {plant}");

        var messages = response.SelectMany(e =>
            TieMapper.CreateTieMessage(e.Message!, PcsTopic.WorkOrderCutoff));
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

        foreach (var batch in mappedMessages.Batch(250)) await SendFamMessages(batch);

        logger.LogDebug("Sent WoCutoff to FAM");
        return $"Sent {mappedMessages.Count} WoCutoff to FAM  for {month} done";

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