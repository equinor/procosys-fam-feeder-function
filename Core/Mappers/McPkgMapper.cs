using System.Text.Json;
using Equinor.TI.Common.Messaging;

namespace Core.Mappers
{
    public static class McPkgMapper
    {
        private const string MilestoneMessageKey = "Milestones";

        public static MessagingObject CreateMcPkgMessagingObject(string messageJson, string messageType, string nameField)
        {
            var deserializedMessage = JsonSerializer.Deserialize<Dictionary<string, object>>(messageJson);

            var mckPgNo = deserializedMessage?.Single(m => m.Key ==nameField).Value.ToString();

            var messagingObject = new MessagingObject { Name = mckPgNo };

          
            var attributeMessages = deserializedMessage?.Where(m => m.Key != MilestoneMessageKey)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            messagingObject.Attributes.Add(new MessagingAttribute
            {
                Name = "Class",
                Value = messageType
            });
            messagingObject.Attributes.AddRange(CommonMapper.ConvertJsonToMessageAttributes(attributeMessages));

            var milestoneDictionary = deserializedMessage?.Where(m => m.Key == MilestoneMessageKey)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            if (milestoneDictionary is not { Count: > 0 and < 2 })
            {
                return messagingObject;
            }

            var (key, value) = milestoneDictionary.Single();
            var mileStoneRelationships = CreateMileStoneRelationships(key,value?.ToString());
            messagingObject.Relationships.Add(mileStoneRelationships);

            return messagingObject;
        }

        private static MessagingRelationship CreateMileStoneRelationships(string name, string milestones)
        {

            return new MessagingRelationship
            {
                Name = name,
                RelatedObjects = CreateMileStoneRelatedObject(milestones)
            };
        }

        private static List<MessagingRelatedObject>? CreateMileStoneRelatedObject(string milestones)
        {
            return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(milestones)?
                .Select(kv
                    => new MessagingRelatedObject
                    {
                        Name ="Milestone",
                        Attributes = kv.Select(pair => new MessagingAttribute
                        {
                            Name = pair.Key,
                            Value = pair.Value.ToString()
                        }).ToList()
                    }).ToList();
        }
    }
}
