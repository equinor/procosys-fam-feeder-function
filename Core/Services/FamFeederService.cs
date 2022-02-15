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

namespace Core.Services;

public class FamFeederService : IFamFeederService
{
    private readonly IEventHubProducerService _eventHubProducerService;
    private readonly IFamEventRepository _repo;
    private readonly ILibRepository _libRepo;
    private readonly CommonLibConfig _config;
    private readonly Regex _rx = new(@"[\a\e\f\n\r\t\v]", RegexOptions.Compiled);

    public FamFeederService(IEventHubProducerService eventHubProducerService, IFamEventRepository repo, ILibRepository libRepo, CommonLibConfig config)
    {
        _eventHubProducerService = eventHubProducerService;
        _repo = repo;
        _libRepo = libRepo;
        _config = config;
    }

    public async void RunFeeder()
    {
        var sw = new Stopwatch();
        sw.Start();

        var mapper = CreateCommonLibMapper();

        //  await WoCutoff(mapper);

        var events = await _repo.GetQuery();
        Console.WriteLine("Found {0} events", events.Count);

        // set the correct messages for each
        var fields = QueryMapping.Query;

        string messageType = fields[0];
        string nameField = fields[1];

        var messages = events.SelectMany(e => CreateTieMessage(e.Message, messageType, nameField));
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).ToList();

        //foreach (var batch in mappedMessages.Batch(250))
        //{
        //    await SendFamMessages(batch);
        //}

        sw.Stop();
        Console.WriteLine("Done {0}", sw.ElapsedMilliseconds);
    }

    private SchemaMapper CreateCommonLibMapper()
    {
        ISchemaSource source = new ApiSource(new ApiSourceOptions
        {
            TokenProviderConnectionString = "RunAs=App;" +
                                            $"AppId={_config.ClientId};" +
                                            $"TenantId={_config.TennantId};" +
                                            $"AppKey={_config.ClientSecret}",
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

    private async Task WoCutoff(ISchemaMapper mapper)
    {
        var tasks = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" }.AsParallel().Select(
            async i =>
            {
                var connectionstring = "";
                var response = await _repo.GetWoCutoffs(i, connectionstring);

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
        string pattern = "[^ -~]+";
        Regex reg_exp = new Regex(pattern);
        busEventMessage = reg_exp.Replace(busEventMessage, "");

        return busEventMessage;
    }
}