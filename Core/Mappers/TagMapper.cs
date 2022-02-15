using System.Text.Json;
using Equinor.TI.Common.Messaging;

namespace Core.Mappers;

public class TagMapper
{

    private const string Register = "RegisterCode";
    private static readonly List<string?> Registers = new() {"ADMINISTRATIVE","CABLE","CIRCUIT_AND_STARTER","CIVIL","DUCTING","ELECTRICAL_FIELD",
        "FIRE_AND_GAS_FIELD","FUNCTION","HEAT_TRACING_CABLE","HOSE_ASSEMBLY","INSTRUMENT_FIELD","JUNCTION_BOX","LINE",
        "MAIN_EQUIPMENT","MANUAL_VALVE","PENETRATION","PIPE_SUPPORT","PIPELINE","SIE","SIE_SUPPLIER","SIGNAL","TELECOM_FIELD"};


    public static IEnumerable<MessagingObject> CreateMessagingObjects(string messageJson,string messageType, string nameField)
    {
        var deserializedMessage = JsonSerializer.Deserialize<Dictionary<string, object>>(messageJson);

        if (deserializedMessage == null)
        {
            throw new Exception("Could not deserialize message");
        }

        var objects = new List<MessagingObject>();

        var dummyTag = deserializedMessage.Any(m => m.Key ==  Register && !Registers.Contains(m.Value.ToString()));
        var name = deserializedMessage.First(m => m.Key == nameField ).Value.ToString();
        if (dummyTag)
        {
            objects.Add(MapToDummyTag(deserializedMessage,name));
            return objects ;
        }

        var tagDetails = deserializedMessage.GetValueOrDefault("TagDetails")?.ToString();
        if (tagDetails != null)
        {
            var identifyingFields = deserializedMessage.Where(att => att.Key is "Plant" or "ProjectName" or "TagNo")
                .ToDictionary(a => a.Key,a=> a.Value);

            var detailsObject = CreateTagDetailsObject(name, identifyingFields, tagDetails);
            objects.Add(detailsObject);
        }

        var messagingObject = new MessagingObject { Name = name };

        messagingObject.Attributes.Add(new MessagingAttribute
        {
            Name = "Class",
            Value = messageType
        });
        messagingObject.Attributes.AddRange(CommonMapper.ConvertJsonToMessageAttributes(deserializedMessage));

        objects.Add(messagingObject);

        return objects;
    }

    private static MessagingObject CreateTagDetailsObject(string? name, Dictionary<string, object>? identifyingFields, string tagDetails)
    {
        var detailsObject = new MessagingObject { Name = name };

        detailsObject.Attributes.Add(new MessagingAttribute
        {
            Name = "Class",
            Value = "TagDetails"
        });

        //tagDetails = tagDetails.Replace("[", "");
        //tagDetails = tagDetails.Replace("]", "");
        
        var details = JsonSerializer.Deserialize<Dictionary<string, object>>(tagDetails);



        detailsObject.Attributes.AddRange(CommonMapper.ConvertJsonToMessageAttributes(identifyingFields));

        detailsObject.Attributes.AddRange(CommonMapper.ConvertJsonToMessageAttributes(details));
        return detailsObject;
    }

    private static MessagingObject MapToDummyTag(Dictionary<string, object> deserializedMessage, string? name)
    {
        var dummyTag = deserializedMessage.First(m => m.Key ==  Register && !Registers.Contains(m.Value.ToString()));

        var messagingObject = new MessagingObject { Name = name };

        messagingObject.Attributes.Add(new MessagingAttribute
        {
            Name = "Class",
            Value = dummyTag+"-Tag"
        });
        messagingObject.Attributes.AddRange(CommonMapper.ConvertJsonToMessageAttributes(deserializedMessage));
        return messagingObject;
    }
}