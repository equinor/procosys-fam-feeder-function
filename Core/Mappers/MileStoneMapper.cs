using System.Text.Json;
using Equinor.TI.Common.Messaging;

namespace Core.Mappers;

public class MileStoneMapper
{

    private const string McPkgMileStoneClassName = "McPkgMilestone";
    private const string CommPkgMileStoneClassName = "CommPkgMilestone";
    private const string McPkgNo = "McPkgNo";
    private const string CommPkgNo = "CommPkgNo";

    public static MessagingObject CreateMileStoneMessagingObject(string messageJson, string nameField)
    {
        var deserializedMessage = JsonSerializer.Deserialize<Dictionary<string, object>>(messageJson);

        if (deserializedMessage == null)
        {
            throw new Exception("Could not deserialize message");
        }

        var name = deserializedMessage.Single(m => m.Key == nameField).Value.ToString();
        var messagingObject = new MessagingObject{Name = name};

        var isMcPkg = deserializedMessage.Any(m => m.Key == McPkgNo && !string.IsNullOrWhiteSpace(m.Value.ToString()));
        var isCommPkg = deserializedMessage.Any(m => m.Key == CommPkgNo && !string.IsNullOrWhiteSpace(m.Value.ToString()));
        Dictionary<string, object>? attributeMessages;
        if (isCommPkg == isMcPkg)
        {
            throw new Exception("MileStone has both or non off CommPkgNo and McPkgNo.");
        }
        if (isCommPkg)
        {
            messagingObject.Attributes.Add(new MessagingAttribute
            {
                Name = "Class",
                Value = CommPkgMileStoneClassName
            });
            attributeMessages = deserializedMessage.Where(m => m.Key != McPkgNo)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        else
        {
            messagingObject.Attributes.Add(new MessagingAttribute
            {
                Name = "Class",
                Value = McPkgMileStoneClassName
            });
            
            attributeMessages = deserializedMessage.Where(m => m.Key != CommPkgNo)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        messagingObject.Attributes.AddRange(CommonMapper.ConvertJsonToMessageAttributes(attributeMessages));

        return messagingObject;
    }

}