﻿using Core.Interfaces;
using Core.Models;
using Equinor.ProCoSys.FamWebJob.Core.Mappers;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.TI.Common.Messaging;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Fam.Core.EventHubs.Contracts;
using Fam.Models.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using Task = System.Threading.Tasks.Task;

namespace Core.Services;

public class FeederService : IFeederService
{
    private readonly CommonLibConfig _commonLibConfig;
    private readonly IEventHubProducerService _eventHubProducerService;
    private ILogger? _logger;
    private readonly IPlantRepository _plantRepository;
    private readonly IEventRepository _repo;
    private readonly IWorkOrderCutoffRepository _cutoffRepository;
    private readonly IServiceBusService _serviceBusService;
    private readonly IDistributedCache _distributedCache;

    public FeederService(IEventHubProducerService eventHubProducerService,
        IEventRepository repo,
        IOptions<CommonLibConfig> commonLibConfig,
        IPlantRepository plantRepository,
        IWorkOrderCutoffRepository cutoffRepository,
        IServiceBusService serviceBusService,
        IDistributedCache distributedCache)
    {
        _eventHubProducerService = eventHubProducerService;
        _repo = repo;
        _plantRepository = plantRepository;
        _cutoffRepository = cutoffRepository;
        _serviceBusService = serviceBusService;
        _commonLibConfig = commonLibConfig.Value;
        _distributedCache = distributedCache;
    }

    public async Task<string> RunFeeder(QueryParameters queryParameters, ILogger logger)
    {
        _logger = logger;
        if (queryParameters.PcsTopic == PcsTopicConstants.WorkOrderCutoff)
            return "Cutoff Should have its own call, this should never happen :D";

        var messagesCount = 0;

        var topic = queryParameters.PcsTopic;
        var plant = queryParameters.Plants.Single();

        try
        {
            var events = await GetEventsBasedOnTopicAndPlant(plant, topic, queryParameters.ShouldAddToQueue,
                queryParameters.CheckAfterDate);

            if (events.Count == 0)
            {
                _logger.LogInformation("found no events for topic {Topic} and plant {Plant}", topic, plant);
                return $"found no events for topic {topic} and plant {plant}";
            }

            _logger.LogInformation(
                "Found {EventCount} events for topic {Topic} and plant {Plant}", events.Count, topic, plant);

            if (queryParameters.ShouldAddToQueue) //Send to Completion
            {
                messagesCount += events.Count;
                await _serviceBusService.SendDataAsync(events, topic);
            }
            else //Send to FAM
            {
                var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e, topic));
                var mapper = CreateCommonLibMapper();
                var mappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m => m.Objects.Any()).ToList();
                if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") != "Development")
                    await SendFamMessages(mappedMessages);
                _logger.LogInformation("Finished sending {Topic} for plant {Plant} to fam", topic, plant);
                messagesCount += mappedMessages.Count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed sending to FAM for plant {plant} topic {topic} with message {ex.Message}");
            return $"Failed sending to FAM for plant {plant} topic {topic} with message {ex.Message}";
        }

        return $"finished successfully sending {messagesCount} messages to for {topic} and plant {plant}";
    }

    public async Task<string> RunForCutoffWeek(string cutoffWeek, string plant, ILogger logger)
    {
        _logger = logger;
        logger.LogInformation("Retrieving events for cutoffWeek: {cutoffWeek} plant: {plant}", cutoffWeek, plant);

        var events = await _repo.GetWoCutoffsByWeekAndPlant(cutoffWeek, plant);

        return await MapAndSendCutoffToFam(cutoffWeek, events, plant);
    }

    public async Task<string> RunForCutoffWeek(string cutoffWeek, IEnumerable<long> projectIds, string plant,
        ILogger logger)
    {
        _logger = logger;

        var events = await _repo.GetWoCutoffsByWeekAndProjectIds(cutoffWeek, plant, projectIds);

        return await MapAndSendCutoffToFam(cutoffWeek, events, plant);
    }

    private async Task<string> MapAndSendCutoffToFam(string cutoffWeek, IReadOnlyCollection<string> events,
        string plant)
    {
        if (events.Count == 0)
        {
            _logger?.LogInformation("found no events, or field is null");
            return "found no events, or field is null";
        }

        _logger?.LogInformation(
            "Found {EventCount} events for WoCutoff for week {CutoffWeek} and plant {Plant} ", events.Count, cutoffWeek,
            plant);

        var messagesCount = 0;

        try
        {
            var messages = events.SelectMany(e => TieMapper.CreateTieMessage(e, PcsTopicConstants.WorkOrderCutoff));
            var mapper = CreateCommonLibMapper();
            var mappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m => m.Objects.Any()).ToList();
            _logger?.LogInformation("Sending {mappedMessagesCount} messages for cutoffWeek: {cutoffWeek} plant: {plant} to Alpha/Fam", mappedMessages.Count(), cutoffWeek, plant);
            await SendFamMessages(mappedMessages);
            messagesCount += mappedMessages.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Failed sending to FAM for plant {Plant} topic {Topic} with message {Message}", plant, PcsTopicConstants.WorkOrderCutoff,ex.Message);
            return
                $"Failed sending to FAM for plant {plant} topic WoCutoff week {cutoffWeek} with message {ex.Message}";
        }
        
        return $"finished successfully sending {messagesCount} messages to fam for WoCutoff for week {cutoffWeek}";
    }

    public Task<List<string>> GetAllPlants()
    {
        return _plantRepository.GetAllPlants();
    }

    public async Task<string> WoCutoff(string plant, string weekNumber, ILogger logger)
    {
        var messagesCount = 0;
        try
        {
            var mapper = CreateCommonLibMapper();
            var response = await _cutoffRepository.GetWoCutoffs(weekNumber, plant);
            logger.LogInformation("Found {EventCount} cutoffs for week number {WeekNumber} in {Plant}", response.Count,
                weekNumber, plant);

            var messages = response.SelectMany(e =>
                TieMapper.CreateTieMessage(e, PcsTopicConstants.WorkOrderCutoff));
            var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

            if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") != "Development")
            {
                foreach (var batch in mappedMessages.Batch(250))
                    await SendFamMessages(batch);
            }

            messagesCount += mappedMessages.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                $"Failed sending to FAM for plant {plant} topic WoCutoff week {weekNumber} with message {ex.Message}");
            return
                $"Failed sending to FAM for plant {plant} topic WoCutoff week {weekNumber} with message {ex.Message}";
        }

        logger.LogInformation("Sent {MappedMessagesCount} WoCutoff to FAM  for {WeekNumber} done in {Plant}",
            messagesCount, weekNumber, plant);
        return $"Sent {messagesCount} WoCutoff to FAM for {weekNumber} done";
    }

    private SchemaMapper CreateCommonLibMapper()
    {
        var source = new ApiSource(new ApiSourceOptions
        {
            TokenProviderConnectionString = "RunAs=App;" +
                                            $"AppId={_commonLibConfig.ClientId};" +
                                            $"TenantId={_commonLibConfig.TenantId};" +
                                            $"AppKey={_commonLibConfig.ClientSecret}"
        });
        
        
       var cacheSource = new DistributedCacheSource(
            _distributedCache,
            source,
            _logger); 
       
        var mapper = new SchemaMapper("ProCoSys_Events", "FAM",  cacheSource);
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

    private async Task<List<string>> GetEventsBasedOnTopicAndPlant(string plant, string topic, bool shouldAddToQueue,
        DateTime? checkAfterDate)
    {
        var events =
            topic switch
            {
                PcsTopicConstants.Action => await _repo.GetActions(plant),
                PcsTopicConstants.CallOff => await _repo.GetCallOffs(plant),
                PcsTopicConstants.Checklist => await _repo.GetCheckLists(plant),
                PcsTopicConstants.CommPkg => await _repo.GetCommPackages(plant),
                PcsTopicConstants.CommPkgMilestone => await _repo.GetCommPkgMilestones(plant),
                PcsTopicConstants.CommPkgOperation => await _repo.GetCommPkgOperations(plant),
                PcsTopicConstants.CommPkgQuery => await _repo.GetCommPkgQueries(plant),
                PcsTopicConstants.CommPkgTask => await _repo.GetCommPkgTasks(plant),
                PcsTopicConstants.Document => shouldAddToQueue
                    ? await _repo.GetDocumentsForCompletion( plant, checkAfterDate)
                    : await _repo.GetDocument(plant),
                PcsTopicConstants.HeatTrace => await _repo.GetHeatTraces(plant),
                PcsTopicConstants.HeatTracePipeTest => await _repo.GetHeatTracePipeTests(plant),
                PcsTopicConstants.Library => shouldAddToQueue
                    ? await _repo.GetLibrariesForPunch(plant)
                    : await _repo.GetLibraries(plant),
                PcsTopicConstants.LibraryField => await _repo.GetLibraryFields(plant),
                PcsTopicConstants.LoopContent => await _repo.GetLoopContents(plant),
                PcsTopicConstants.McPkg => await _repo.GetMcPackages(plant),
                PcsTopicConstants.McPkgMilestone => await _repo.GetMcPkgMilestones(plant),
                PcsTopicConstants.PipingRevision => await _repo.GetPipingRevisions(plant),
                PcsTopicConstants.PipingSpool => await _repo.GetPipingSpools(plant),
                PcsTopicConstants.Project => await _repo.GetProjects(plant),
                PcsTopicConstants.PunchListItem => shouldAddToQueue
                    ? await _repo.GetPunchItemsForCompletion(plant, checkAfterDate)
                    : await _repo.GetPunchItems(plant),
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
                PcsTopicConstants.WorkOrder => shouldAddToQueue
                    ? await _repo.GetWorkOrdersForCompletion(plant, checkAfterDate)
                    : await _repo.GetWorkOrders(plant),
                PcsTopicConstants.WoChecklist => await _repo.GetWoChecklists(plant),
                PcsTopicConstants.WoMaterial => await _repo.GetWoMaterials(plant),
                PcsTopicConstants.WoMilestone => await _repo.GetWoMilestones(plant),
                //Person table does not have projectschema, so we ignore plant input
                PcsTopicConstants.Person => shouldAddToQueue
                    ? await _repo.GetPersonsForPunch()
                    : throw new Exception("Only applicable for 'addToQueue'"),
                "PunchItemHistory" => shouldAddToQueue
                    ? await _repo.GetPunchItemHistory(plant, checkAfterDate)
                    : throw new Exception("Only applicable for 'addToQueue'"),
                "PunchItemComment" => shouldAddToQueue
                    ? await _repo.GetPunchItemComments(plant, checkAfterDate)
                    : throw new Exception("Only applicable for 'addToQueue'"),
                "PunchItemAttachment" => shouldAddToQueue
                    ? await _repo.GetAttachmentsForCompletion(plant, checkAfterDate)
                    : throw new Exception("Only applicable for 'addToQueue'"),
                PcsTopicConstants.PunchPriorityLibraryRelation => shouldAddToQueue
                    ? await _repo.GetPunchPriorityLibRelations(plant)
                    : throw new Exception("Only applicable for 'addToQueue'"),
                PcsTopicConstants.Notification => await _repo.GetNotifications(plant),
                PcsTopicConstants.NotificationCommPkg => await _repo.GetNotificationCommPkgs(plant),
                PcsTopicConstants.NotificationWorkOrder => await _repo.GetNotificationWorkOrders(plant),
                PcsTopicConstants.NotificationSignature => await _repo.GetNotificationSignatures(plant),

                _ => Default(topic)
            };
        return events.ToList();
    }

    private List<string> Default(string topic)
    {
        _logger?.LogInformation("{Topic} not included in switch statement", topic);
        return new List<string>();
    }
}