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
            "Found {Events} events for topic {Topic} and plant {Plant}",events.Count,queryParameters.PcsTopic,queryParameters.Plant);
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
            "Found {Events} events for WoCutoff for week {CutoffWeek} and plant {Plant} ", events.Count, cutoffWeek, plant);

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
        logger.LogInformation("Found {count} cutoffs for month {month} in {plant}",response.Count,month,plant);

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

        // Add caching functionality
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
        var events = new List<string>();
        var plant = queryParameters.Plant;
        switch (queryParameters.PcsTopic)
        {
            case PcsTopicConstants.SwcrAttachment:
                events = await _repo.GetSwcrAttachments(plant);
                break;
            case nameof(PcsTopicConstants.SwcrType):
                events = await _repo.GetSwcrType(plant);
                break;
            case nameof(PcsTopicConstants.SwcrOtherReference):
                events = await _repo.GetSwcrOtherReferences(plant);
                break;
            case nameof(PcsTopicConstants.Action):
                events = await _repo.GetActions(plant);
                break;
            case nameof(PcsTopicConstants.CommPkgTask):
                events = await _repo.GetCommPkgTasks(plant);
                break;
            case nameof(PcsTopicConstants.Task):
                events = await _repo.GetTasks(plant);
                break;
            case nameof(PcsTopicConstants.CommPkg):
                events = await _repo.GetCommPackages(plant);
                break;
            case nameof(PcsTopicConstants.Ipo):
                break;
            case nameof(PcsTopicConstants.McPkg):
                events = await _repo.GetMcPackages(plant);
                break;
            case nameof(PcsTopicConstants.Project):
                events = await _repo.GetProjects(plant);
                break;
            case nameof(PcsTopicConstants.Responsible):
                events = await _repo.GetResponsible(plant);
                break;
            case nameof(PcsTopicConstants.Tag):
                events = await _repo.GetTags(plant);
                break;
            case nameof(PcsTopicConstants.PunchListItem):
                events = await _repo.GetPunchItems(plant);
                break;
            case nameof(PcsTopicConstants.Library):
                events = await _repo.GetLibrary(plant);
                break;
            case nameof(PcsTopicConstants.WorkOrder):
                events = await _repo.GetWorkOrders(plant);
                break;
            case nameof(PcsTopicConstants.Checklist):
                events = await _repo.GetCheckLists(plant);
                break;
            case nameof(PcsTopicConstants.Milestone):
                events = await _repo.GetMcPkgMilestones(plant);
                break;
            case nameof(PcsTopicConstants.WoChecklist):
                events = await _repo.GetWoChecklists(plant);
                break;
            case nameof(PcsTopicConstants.SWCR):
                events = await _repo.GetSwcr(plant);
                break;
            case nameof(PcsTopicConstants.SWCRSignature):
                events = await _repo.GetSwcrSignature(plant);
                break;
            case nameof(PcsTopicConstants.PipingRevision):
                events = await _repo.GetPipingRevision(plant);
                break;
            case nameof(PcsTopicConstants.PipingSpool):
                events = await _repo.GetPipingSpool(plant);
                break;
            case nameof(PcsTopicConstants.WoMaterial):
                events = await _repo.GetWoMaterials(plant);
                break;
            case nameof(PcsTopicConstants.Stock):
                events = await _repo.GetStock(plant);
                break;
            case nameof(PcsTopicConstants.WoMilestone):
                events = await _repo.GetWoMilestones(plant);
                break;
            case nameof(PcsTopicConstants.CommPkgOperation):
                events = await _repo.GetCommPkgOperations(plant);
                break;
            case nameof(PcsTopicConstants.Document):
                events = await _repo.GetDocument(plant);
                break;
            case nameof(PcsTopicConstants.LoopContent):
                events = await _repo.GetLoopContent(plant);
                break;
            case nameof(PcsTopicConstants.Query):
                events = await _repo.GetQuery(plant);
                break;
            case nameof(PcsTopicConstants.QuerySignature):
                events = await _repo.GetQuerySignature(plant);
                break;
            case nameof(PcsTopicConstants.CallOff):
                events = await _repo.GetCallOff(plant);
                break;
            case nameof(PcsTopicConstants.CommPkgQuery):
                events = await _repo.GetCommPkgQuery(plant);
                break;
            case nameof(PcsTopicConstants.HeatTrace):
                events = await _repo.GetHeatTrace(plant);
                break;
            case nameof(PcsTopicConstants.LibraryField):
                events = await  _repo.GetLibraryField(plant);
                break;

            default:
            {
                _logger?.LogInformation("{topic} not included in switch statement",queryParameters.PcsTopic);
                break;
            }
        }
        return events;
    }
}