using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618

public class TaskEvent : ITaskEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid? TaskParentProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public int DocumentId { get; init; }
    public string Title { get; init; }
    public string TaskId { get; init; }
    public Guid ElementContentGuid { get; init; }
    public string Description { get; init; }
    public string Comments { get; init; }
    public DateTime LastUpdated { get; init; }
    public DateTime? SignedAt { get; init; }
    public Guid? SignedBy { get; init; }
}