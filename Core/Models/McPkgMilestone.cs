using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618

public class McPkgMilestone : IMcPkgMilestoneEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string? PlantName { get; init; }
    public string? ProjectName { get; init; }
    public Guid McPkgGuid { get; init; }
    public string? McPkgNo { get; init; }
    public string Code { get; init; }
    public DateTime? ActualDate { get; init; }
    public DateOnly? PlannedDate { get; init; }
    public DateOnly? ForecastDate { get; init; }
    public string? Remark { get; init; }
    public bool? IsSent { get; init; }
    public bool? IsAccepted { get; init; }
    public bool? IsRejected { get; init; }
    public DateTime LastUpdated { get; init; }
}