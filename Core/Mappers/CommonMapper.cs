using System.Text.Json;
using Equinor.TI.Common.Messaging;

namespace Core.Mappers
{
    public static class CommonMapper
    {
        public static MessagingObject CreateMessagingObject(string messageJson, string messageType, string nameField)
        {
            var attributeMessages = JsonSerializer.Deserialize<Dictionary<string, object>>(messageJson);
            var name = attributeMessages?.First(m => m.Key == nameField ).Value.ToString();

            var messagingObject = new MessagingObject { Name = name };

            messagingObject.Attributes.Add(new MessagingAttribute
            {
                Name = "Class",
                Value = messageType
            });
            messagingObject.Attributes.AddRange(CommonMapper.ConvertJsonToMessageAttributes(attributeMessages));

            return messagingObject;
        }

        internal static IEnumerable<MessagingAttribute> ConvertJsonToMessageAttributes(
            Dictionary<string, object>? deserializedMessage)
        {
            return deserializedMessage
                .Select(d => new MessagingAttribute
                {
                    Name = d.Key,
                    Value = d.Value.ToString()
                }).Where(n => n.Name != "ProjectNames");
        }

    }
}
