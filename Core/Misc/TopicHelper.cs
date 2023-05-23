using System.Reflection;
using Equinor.ProCoSys.PcsServiceBus;

namespace Core.Misc;

public static class TopicHelper
{
    public static IEnumerable<string> GetAllTopicsAsEnumerable()
    {
        return typeof(PcsTopicConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(x => x.GetRawConstantValue()?.ToString())
            .Where(t => t != PcsTopicConstants.WorkOrderCutoff && t != PcsTopicConstants.Document)
            .Select(t => t.ToString());
    }
}