using System.Reflection;
using Equinor.ProCoSys.PcsServiceBus;

namespace Core.Misc;

public static class TopicHelper
{
    public static IEnumerable<string> GetAllTopicsAsEnumerable()
    {
        return typeof(PcsTopicConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi is { IsLiteral: true, IsInitOnly: false })
            .Select(x => x.GetRawConstantValue()?.ToString())
            .Where(t => t is not PcsTopicConstants.WorkOrderCutoff and not PcsTopicConstants.Document and not PcsTopicConstants.Milestone)
            .Select(t => t!.ToString());
    }
}