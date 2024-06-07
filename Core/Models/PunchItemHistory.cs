using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;

public struct PunchItemHistory : IHasEventType
{
    public Guid ProCoSysGuid { get; init; }
    public Guid PunchItemGuid { get; init; }
    public string Plant { get; init; }
    public string FieldName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string? OldValueLong { get; init; }
    public string? NewValueLong { get; init; }
    public DateTime ChangedAt { get; init; }
    public string ChangedBy { get; init; }
    public string EventType { get; init; } 
}