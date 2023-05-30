using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class Action : IActionEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid ElementContentGuid { get; init; }
    public string? CommPkgNo { get; init; }
    public Guid? CommPkgGuid { get; init; }
    public string? SwcrNo { get; init; }
    public Guid? SwcrGuid { get; init; }
    public string? DocumentNo { get; init; }
    public string? Description { get; init; }
    public Guid? DocumentGuid { get; init; }
    public string ActionNo { get; init; }
    public string? Title { get; init; }
    public string? Comments { get; init; }
    public DateOnly? Deadline { get; init; }
    public string? CategoryCode { get; init; }
    public Guid? CategoryGuid { get; init; }
    public string? PriorityCode { get; init; }
    public Guid? PriorityGuid { get; init; }
    public Guid? RequestedByOid { get; init; }
    public Guid? ActionByOid { get; init; }
    public string? ActionByRole { get; init; }
    public Guid? ActionByRoleGuid { get; init; }
    public Guid? ResponsibleOid { get; init; }
    public string? ResponsibleRole { get; init; }
    public Guid? ResponsibleRoleGuid { get; init; }
    public DateTime LastUpdated { get; init; }
    public DateTime? SignedAt { get; init; }
    public Guid? SignedBy { get; init; }
}