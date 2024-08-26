using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using JetBrains.Annotations;

namespace Core.Models;
#pragma warning disable CS8618
[UsedImplicitly]
public record PunchPriorityLibRelation : IPunchPriorityLibRelationEventV1
{

    public string EventType { get; init; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init;}
    public Guid CommPriorityGuid { get; init; }
    public DateTime LastUpdated { get; init; }
}