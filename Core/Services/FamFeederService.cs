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
using Task = System.Threading.Tasks.Task;

namespace Core.Services;

public class FamFeederService : IFamFeederService
{
    private readonly CommonLibConfig _commonLibConfig;
    private readonly IEventHubProducerService _eventHubProducerService;
    private ILogger? _logger;
    private readonly IPlantRepository _plantRepository;
    private readonly IFamEventRepository _repo;
    private readonly IWorkOrderCutoffRepository _cutoffRepository;

    public FamFeederService(IEventHubProducerService eventHubProducerService, 
        IFamEventRepository repo,
        IOptions<CommonLibConfig> commonLibConfig,
        IPlantRepository plantRepository, IWorkOrderCutoffRepository cutoffRepository)
    {
        _eventHubProducerService = eventHubProducerService;
        _repo = repo;
        _plantRepository = plantRepository;
        _cutoffRepository = cutoffRepository;
        _commonLibConfig = commonLibConfig.Value;
    }

    public async Task<string> RunFeeder(QueryParameters queryParameters, ILogger logger)
    {
        _logger = logger;
        
        if (queryParameters.PcsTopic == PcsTopicConstants.WorkOrderCutoff)
        {
            return "Cutoff Should have its own call, this should never happen :D";
        }

        var events = await GetEventsBasedOnTopicAndPlant(queryParameters);

        if (events.Count == 0)
        {
            _logger.LogInformation("found no events, or field is null");
            return $"found no events of type {queryParameters.PcsTopic}, or field is null for {queryParameters.Plant}";
        }

        _logger.LogInformation(
            "Found {EventCount} events for topic {Topic} and plant {Plant}",events.Count,queryParameters.PcsTopic,queryParameters.Plant);
        var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e, queryParameters.PcsTopic));
        var mapper = CreateCommonLibMapper();
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m=> m.Objects.Any()).ToList();
        
        if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") != "Development")
        {
            await SendFamMessages(mappedMessages);
        }

        _logger.LogInformation("Finished sending {Topic} to fam",queryParameters.PcsTopic);

        return $"finished successfully sending {mappedMessages.Count} messages to fam for {queryParameters.PcsTopic} and plant {queryParameters.Plant}";
    }

    public async Task<string> RunForCutoffWeek(string cutoffWeek, string plant, ILogger logger)
    {        
        _logger = logger;

        var events = await _repo.GetWoCutoffsByWeekAndPlant(cutoffWeek, plant);

        if (events.Count == 0)
        {
            _logger.LogInformation("found no events, or field is null");
            return "found no events, or field is null";
        }

        _logger.LogInformation(
            "Found {EventCount} events for WoCutoff for week {CutoffWeek} and plant {Plant} ", events.Count, cutoffWeek, plant);

        var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e, PcsTopicConstants.WorkOrderCutoff));
        var mapper = CreateCommonLibMapper();
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m => m.Objects.Any()).ToList();

        await SendFamMessages(mappedMessages);

        return $"finished successfully sending {mappedMessages.Count} messages to fam for WoCutoff for week {cutoffWeek}";
    }

    public Task<List<string>> GetAllPlants() => _plantRepository.GetAllPlants();

    public async Task<string> WoCutoff(string plant, string month, ILogger logger)
    {
        var mapper = CreateCommonLibMapper();
        var response = await _cutoffRepository.GetWoCutoffs(month, plant);
        logger.LogInformation("Found {EventCount} cutoffs for month {Month} in {Plant}",response.Count,month,plant);

        var messages = response.SelectMany(e =>
            TieMapper.CreateTieMessage(e, PcsTopicConstants.WorkOrderCutoff));
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

        foreach (var batch in mappedMessages.Batch(250))
        {
            await SendFamMessages(batch);
        }

        logger.LogDebug("Sent WoCutoff to FAM");
        return $"Sent {mappedMessages.Count} WoCutoff to FAM  for {month} done";
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

        // Adds caching functionality
        source = new CacheWrapper(
            source,
            maxCacheAge: TimeSpan.FromDays(1), // Use TimeSpan.Zero for no recache based on age
            checkForChangesFrequency: TimeSpan
                .FromHours(1)); // Use TimeSpan.Zero when cache should never check for changes.

        var mapper = new SchemaMapper("ProCoSys_Events", "FAM", source);
        return mapper;
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
    private async Task<List<string>> GetEventsBasedOnTopicAndPlant(QueryParameters queryParameters)
    {
        var plant = queryParameters.Plant;
        return queryParameters.PcsTopic switch
        {
            PcsTopicConstants.Action => await _repo.GetActions(plant),
            PcsTopicConstants.SwcrAttachment => await _repo.GetSwcrAttachments(plant),
            PcsTopicConstants.SwcrType => await _repo.GetSwcrType(plant),
            PcsTopicConstants.SwcrOtherReference => await _repo.GetSwcrOtherReferences(plant),
            PcsTopicConstants.CommPkgTask => await _repo.GetCommPkgTasks(plant),
            PcsTopicConstants.Task => await _repo.GetTasks(plant),
            PcsTopicConstants.CommPkg => await _repo.GetCommPackages(plant),
            PcsTopicConstants.McPkg => await _repo.GetMcPackages(plant),
            PcsTopicConstants.Project => await _repo.GetProjects(plant),
            PcsTopicConstants.Responsible => await _repo.GetResponsible(plant),
            PcsTopicConstants.Tag => await _repo.GetTags(plant),
            PcsTopicConstants.TagEquipment => await _repo.GetTagEquipments(plant),
            PcsTopicConstants.PunchListItem => await _repo.GetPunchItems(plant),
            PcsTopicConstants.Library => await _repo.GetLibrary(plant),
            PcsTopicConstants.WorkOrderCutoff => await _repo.GetWorkOrders(plant),
            PcsTopicConstants.Checklist => await _repo.GetCheckLists(plant),
            PcsTopicConstants.McPkgMilestone => await _repo.GetMcPkgMilestones(plant),
            PcsTopicConstants.WoChecklist => await _repo.GetWoChecklists(plant),
            PcsTopicConstants.SWCR => await _repo.GetSwcr(plant),
            PcsTopicConstants.SWCRSignature => await _repo.GetSwcrSignature(plant),
            PcsTopicConstants.PipingRevision => await _repo.GetPipingRevision(plant),
            PcsTopicConstants.PipingSpool => await _repo.GetPipingSpool(plant),
            PcsTopicConstants.WoMaterial => await _repo.GetWoMaterials(plant),
            PcsTopicConstants.Stock => await _repo.GetStock(plant),
            PcsTopicConstants.WoMilestone => await _repo.GetWoMilestones(plant),
            PcsTopicConstants.CommPkgOperation => await _repo.GetCommPkgOperations(plant),
            PcsTopicConstants.CommPkgMilestone => await _repo.GetCommPkgMilestones(plant),
            PcsTopicConstants.Document => await _repo.GetDocument(plant),
            PcsTopicConstants.LoopContent => await _repo.GetLoopContent(plant),
            PcsTopicConstants.Query => await _repo.GetQuery(plant),
            PcsTopicConstants.QuerySignature => await _repo.GetQuerySignature(plant),
            PcsTopicConstants.CallOff => await _repo.GetCallOff(plant),
            PcsTopicConstants.CommPkgQuery => await _repo.GetCommPkgQuery(plant),
            PcsTopicConstants.HeatTrace => await _repo.GetHeatTrace(plant),
            PcsTopicConstants.LibraryField => await _repo.GetLibraryField(plant),
            var topic => Default(topic)
        };
    }

    private  List<string> Default(string topic)
    {
        _logger?.LogInformation("{Topic} not included in switch statement",topic);
        return new List<string>();
    }
}