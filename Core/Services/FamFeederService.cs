using System.Collections;
using Core.Interfaces;
using Core.Mappers;
using Equinor.TI.Common.Messaging;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Fam.Core.EventHubs.Contracts;
using Fam.Models.Exceptions;
using MoreLinq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Extensions.Options;

namespace Core.Services;

public class FamFeederService : IFamFeederService
{
    private readonly IEventHubProducerService _eventHubProducerService;
    private readonly IFamEventRepository _repo;
    private readonly IPlantRepository _plantRepository;
    private readonly CommonLibConfig _commonLibConfig;
    private readonly FamFeederOptions _famFeederOptions;
    private readonly Regex _rx = new(@"[\a\e\f\n\r\t\v]", RegexOptions.Compiled);

    public FamFeederService(IEventHubProducerService eventHubProducerService, IFamEventRepository repo,
        IOptions<CommonLibConfig> commonLibConfig, IOptions<FamFeederOptions> famFeederOptions, IPlantRepository plantRepository)
    {
        _eventHubProducerService = eventHubProducerService;
        _repo = repo;
        _plantRepository = plantRepository;
        _commonLibConfig = commonLibConfig.Value;
        _famFeederOptions = famFeederOptions.Value;
    }

    public async Task RunFeeder(QueryParameters queryParameters)
    {
        var sw = new Stopwatch();
        sw.Start();

        var mapper = CreateCommonLibMapper();
        if (queryParameters.PcsTopic == PcsTopic.WorkOrderCutoff)
        {
            await WoCutoff(mapper,queryParameters.Plant);
            return;
        }

        //  await WoCutoff(mapper);
        var events = new List<FamEvent>();
        var fields = new string[2];
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

        if (events.Count == 0)
        {
            return;
        }


        Console.WriteLine("Found {0} events", events.Count);

        // set the correct messages for each
        //var fields = QueryMapping.Query;

        var messageType = fields[0];
        var nameField = fields[1];

        var messages = events.SelectMany(e => CreateTieMessage(e.Message, messageType, nameField));
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

        foreach (var batch in mappedMessages.Batch(250))
        {
            await SendFamMessages(batch);
        }

        sw.Stop();
        Console.WriteLine("Done {0}", sw.ElapsedMilliseconds);
    }

    public Task<List<string>> GetAllPlants() => _plantRepository.GetAllPlants();

    private SchemaMapper CreateCommonLibMapper()
    {
        ISchemaSource source = new ApiSource(new ApiSourceOptions
        {
            TokenProviderConnectionString = "RunAs=App;" +
                                            $"AppId={_commonLibConfig.ClientId};" +
                                            $"TenantId={_commonLibConfig.TenantId};" +
                                            $"AppKey={_commonLibConfig.ClientSecret}",
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
        var tasks = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" }.AsParallel().Select(
            async i =>
            {
                var connectionString = _famFeederOptions.ProCoSysConnectionString;
                var response = await _repo.GetWoCutoffs(i,plant, connectionString);

                var messages = response.SelectMany(e => CreateTieMessage(e.Message, "WorkOrderCutoff", "WoNo"));
                messages = messages.Select(m => mapper.Map(m).Message).ToList();

                foreach (var batch in messages.Batch(250))
                {
                    await SendFamMessages(batch);
                }
            });
        await Task.WhenAll(tasks);
    }

    private async Task SendFamMessages(IEnumerable<Message> messages)
    {
        try
        {
            //Console.WriteLine("Did smt");
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

    private IEnumerable<Message> CreateTieMessage(string messageJson, string messageType, string nameField)
    {
        var message = CreateEmptyMessage();
        messageJson = WashString(messageJson);

        var messages = new List<Message>();

        switch (messageType)
        {
            case "Milestone":

                message.Objects.Add(MileStoneMapper.CreateMileStoneMessagingObject(messageJson, nameField));
                messages.Add(message);
                break;

            case "Tag":
                {
                    var tagMessages = TagMapper.CreateMessagingObjects(messageJson, messageType, nameField);
                    tagMessages.ForEach(mo =>
                    {
                        var m = CreateEmptyMessage();
                        m.Objects.Add(mo);
                        messages.Add(m);
                    });
                    break;
                }

            default:
                message.Objects.Add(CommonMapper.CreateMessagingObject(messageJson, messageType, nameField));
                messages.Add(message);
                break;
        }

        return messages;
    }

    private static Message CreateEmptyMessage()
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now
        };
    }

    private string WashString(string busEventMessage)
    {
        busEventMessage = busEventMessage.Replace("\r", "");
        busEventMessage = busEventMessage.Replace("\n", "");
        busEventMessage = busEventMessage.Replace("\t", "");
        busEventMessage = busEventMessage.Replace("\f", "");
        busEventMessage = _rx.Replace(busEventMessage, m => Regex.Escape(m.Value));

        ////Removes non printable characters
        const string pattern = "[^ -~]+";
        var regExp = new Regex(pattern);
        busEventMessage = regExp.Replace(busEventMessage, "");

        return busEventMessage;
    }
}