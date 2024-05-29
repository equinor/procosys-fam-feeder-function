using Azure.Messaging.ServiceBus;
using Core.Interfaces;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Extensions.Options;
using InvalidOperationException = System.InvalidOperationException;

namespace Core.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly Dictionary<string,ServiceBusSender> _senders = new();

    public ServiceBusService(IOptionsMonitor<ServiceBusOptions> optionsMonitor)
    {
        var serviceBusConnectionString = optionsMonitor.CurrentValue.ServiceBusConnectionString;
         _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
    }
    
    public async Task SendDataAsync(IEnumerable<string> data, string topic)
    {
        /***
         * group messages into batches, and fail before sending if message exceeds size limit
         *
         * Pattern taken from:
         * https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/MigrationGuide.md
         */
        
        if(!_senders.TryGetValue(topic, out _))
        {
            _senders.TryAdd(topic,_serviceBusClient.CreateSender(GetQueueName(topic)));
        }
        var serviceBusSender = _senders[topic];

        Queue<ServiceBusMessage> messages = new();
        data.ToList().ForEach(m => messages.Enqueue(new ServiceBusMessage(m)));
        var messageCount = messages.Count;
        
        while (messages.Count > 0)
        {
            using var messageBatch = await serviceBusSender.CreateMessageBatchAsync();
            // add first unsent message to batch
            if (messageBatch.TryAddMessage(messages.Peek()))
            {
                messages.Dequeue();
            }
            else
            {
                // if the first message can't fit, then it is too large for the batch
                throw new Exception($"Message {messageCount - messages.Count} is too large and cannot be sent.");
            }

            // add as many messages as possible to the current batch
            while (messages.Count > 0 && messageBatch.TryAddMessage(messages.Peek()))
            {
                messages.Dequeue();
            }
            await serviceBusSender.SendMessagesAsync(messageBatch);
        }
    }

    private static string GetQueueName(string topic)
    {
        return topic switch
        {
            PcsTopicConstants.PunchListItem => "punchitemcompletiontransferqueue",
            PcsTopicConstants.Library => "librarycompletiontransferqueue",
            PcsTopicConstants.Document => "documentcompletiontransferqueue",
            PcsTopicConstants.SWCR => "swcrcompletiontransferqueue",
            PcsTopicConstants.WorkOrder => "workordercompletiontransferqueue",
            PcsTopicConstants.Project => "projectcompletiontransferqueue",
            PcsTopicConstants.Person => "personcompletiontransferqueue",
            _ => throw new InvalidOperationException()
        };
    }
}
