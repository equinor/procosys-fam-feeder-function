using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class Swcr : ISwcrEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string? ProjectName { get; init; }
    public string SwcrNo { get; init; }
    public long SwcrId { get; init; }
    public Guid? CommPkgGuid { get; init; }
    public string? CommPkgNo { get; init; }
    public string? Description { get; init; }
    public string? Modification { get; init; }
    public string? Priority { get; init; }
    public string? System { get; init; }
    public string? ControlSystem { get; init; }
    public string? Contract { get; init; }
    public string? Supplier { get; init; }
    public string? Node { get; init; }
    public string? Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsVoided { get; init; }
    public DateTime LastUpdated { get; init; }
    public DateOnly? DueDate { get; init; }
    public float? EstimatedManHours { get; init; }
}