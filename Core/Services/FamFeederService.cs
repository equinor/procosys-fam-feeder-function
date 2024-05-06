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

        if (queryParameters.PcsTopics.Contains(PcsTopicConstants.WorkOrderCutoff, StringComparer.InvariantCultureIgnoreCase))
        {
            return "Cutoff Should have its own call, this should never happen :D";
        }

        var mappedMessagesCount = 0;

        foreach (var plant in queryParameters.Plants)
        {
            foreach (var topic in queryParameters.PcsTopics)
            {
                var events = await GetEventsBasedOnTopicAndPlant(plant, topic);

                if (events.Count == 0)
                {
                    _logger.LogInformation("found no events for topic {Topic} and plant {Plant}", topic, plant);
                    continue;
                }

                _logger.LogInformation(
                    "Found {EventCount} events for topic {Topic} and plant {Plant}",events.Count,topic,plant);
                var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e, topic));
                var mapper = CreateCommonLibMapper();
                var mappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m=> m.Objects.Any()).ToList();

                if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") != "Development")
                {
                    await SendFamMessages(mappedMessages);
                }

                _logger.LogInformation("Finished sending {Topic} for plant {Plant} to fam", topic, plant);
                mappedMessagesCount += mappedMessages.Count;
            }
        }

        return $"finished successfully sending {mappedMessagesCount} messages to fam for {string.Join(",", queryParameters.PcsTopics)} and plants {string.Join(",", queryParameters.Plants)}";
    }

    public async Task<string> RunForCutoffWeek(string cutoffWeek, string plant, ILogger logger)
    {
        _logger = logger;

        var events = await _repo.GetWoCutoffsByWeekAndPlant(cutoffWeek, plant);

        return await MapAndSendCutoffToFam(cutoffWeek, events, plant);
    }

    public async Task<string> RunForCutoffWeek(string cutoffWeek, IEnumerable<long> projectIds, string plant, ILogger logger)
    {
        _logger = logger;

        var events = await _repo.GetWoCutoffsByWeekAndProjectIds(cutoffWeek, plant,projectIds);

        return await MapAndSendCutoffToFam(cutoffWeek, events, plant);
    }

    private async Task<string> MapAndSendCutoffToFam(string cutoffWeek, IReadOnlyCollection<string> events, string plant)
    {
        if (events.Count == 0)
        {
            _logger?.LogInformation("found no events, or field is null");
            return "found no events, or field is null";
        }

        _logger?.LogInformation(
            "Found {EventCount} events for WoCutoff for week {CutoffWeek} and plant {Plant} ", events.Count, cutoffWeek,
            plant);

        var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e, PcsTopicConstants.WorkOrderCutoff));
        var mapper = CreateCommonLibMapper();
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m => m.Objects.Any()).ToList();

        await SendFamMessages(mappedMessages);

        return $"finished successfully sending {mappedMessages.Count} messages to fam for WoCutoff for week {cutoffWeek}";
    }




    public Task<List<string>> GetAllPlants() => _plantRepository.GetAllPlants();

    public async Task<string> WoCutoff(string plant, string weekNumber, ILogger logger)
    {
        var mapper = CreateCommonLibMapper();
        var response = await _cutoffRepository.GetWoCutoffs(weekNumber, plant);
        logger.LogInformation("Found {EventCount} cutoffs for week number {WeekNumber} in {Plant}",response.Count,weekNumber,plant);

        var messages = response.SelectMany(e =>
            TieMapper.CreateTieMessage(e, PcsTopicConstants.WorkOrderCutoff));
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

        if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") != "Development")
        {
            foreach (var batch in mappedMessages.Batch(250))
            {
                await SendFamMessages(batch);
            }
        }

        logger.LogInformation("Sent {MappedMessagesCount} WoCutoff to FAM  for {WeekNumber} done in {Plant}", mappedMessages.Count, weekNumber, plant);
        return $"Sent {mappedMessages.Count} WoCutoff to FAM  for {weekNumber} done";
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

    private async Task<List<string>> GetEventsBasedOnTopicAndPlant(string plant, string topic)
    {
        return await GetEventsBasedOnTopicAndPlant(new QueryParameters(plant, topic));
    }

    private async Task<List<string>> GetEventsBasedOnTopicAndPlant(QueryParameters queryParameters)
    {
        var returnEvents = new List<string>();
        foreach (var plant in queryParameters.Plants)
        {
            foreach (var topic in queryParameters.PcsTopics)
            {
                var events = topic switch
                {
                    PcsTopicConstants.Action => await _repo.GetActions(plant),
                    PcsTopicConstants.CallOff => await _repo.GetCallOffs(plant),
                    PcsTopicConstants.Checklist => await _repo.GetCheckLists(plant),
                    PcsTopicConstants.CommPkg => await _repo.GetCommPackages(plant),
                    PcsTopicConstants.CommPkgMilestone => await _repo.GetCommPkgMilestones(plant),
                    PcsTopicConstants.CommPkgOperation => await _repo.GetCommPkgOperations(plant),
                    PcsTopicConstants.CommPkgQuery => await _repo.GetCommPkgQueries(plant),
                    PcsTopicConstants.CommPkgTask => await _repo.GetCommPkgTasks(plant),
                    PcsTopicConstants.Document => await _repo.GetDocument(plant),
                    PcsTopicConstants.HeatTrace => await _repo.GetHeatTraces(plant),
                    PcsTopicConstants.HeatTracePipeTest => await _repo.GetHeatTracePipeTests(plant),
                    PcsTopicConstants.Library => await _repo.GetLibraries(plant),
                    PcsTopicConstants.LibraryField => await _repo.GetLibraryFields(plant),
                    PcsTopicConstants.LoopContent => await _repo.GetLoopContents(plant),
                    PcsTopicConstants.McPkg => await _repo.GetMcPackages(plant),
                    PcsTopicConstants.McPkgMilestone => await _repo.GetMcPkgMilestones(plant),
                    PcsTopicConstants.PipingRevision => await _repo.GetPipingRevisions(plant),
                    PcsTopicConstants.PipingSpool => await _repo.GetPipingSpools(plant),
                    PcsTopicConstants.Project => await _repo.GetProjects(plant),
                    PcsTopicConstants.PunchListItem => await _repo.GetPunchItems(plant),
                    PcsTopicConstants.Query => await _repo.GetQueries(plant),
                    PcsTopicConstants.QuerySignature => await _repo.GetQuerySignatures(plant),
                    PcsTopicConstants.Responsible => await _repo.GetResponsibles(plant),
                    PcsTopicConstants.Stock => await _repo.GetStocks(plant),
                    PcsTopicConstants.SWCR => await _repo.GetSwcrs(plant),
                    PcsTopicConstants.SwcrAttachment => await _repo.GetSwcrAttachments(plant),
                    PcsTopicConstants.SwcrOtherReference => await _repo.GetSwcrOtherReferences(plant),
                    PcsTopicConstants.SWCRSignature => await _repo.GetSwcrSignatures(plant),
                    PcsTopicConstants.SwcrType => await _repo.GetSwcrTypes(plant),
                    PcsTopicConstants.Tag => await _repo.GetTags(plant),
                    PcsTopicConstants.TagEquipment => await _repo.GetTagEquipments(plant),
                    PcsTopicConstants.Task => await _repo.GetTasks(plant),
                    PcsTopicConstants.WorkOrder => await _repo.GetWorkOrders(plant),
                    PcsTopicConstants.WoChecklist => await _repo.GetWoChecklists(plant),
                    PcsTopicConstants.WoMaterial => await _repo.GetWoMaterials(plant),
                    PcsTopicConstants.WoMilestone => await _repo.GetWoMilestones(plant),
                    var defaultTopic => Default(defaultTopic)
                };

                returnEvents.AddRange(events);
            }
        }

        return returnEvents;
    }

    private  List<string> Default(string topic)
    {
        _logger?.LogInformation("{Topic} not included in switch statement",topic);
        return new List<string>();
    }
}