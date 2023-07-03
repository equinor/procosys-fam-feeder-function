using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class McPkg : IMcPkgEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string PlantName { get; init; }
    public string ProjectName { get; init; }
    public string McPkgNo { get; init; }
    public long McPkgId { get; init; }
    public string CommPkgNo { get; init; }
    public Guid CommPkgGuid { get; init; }
    public string? Description { get; init; }
    public string? Remark { get; init; }
    public string ResponsibleCode { get; init; }
    public string? ResponsibleDescription { get; init; }
    public string? AreaCode { get; init; }
    public string? AreaDescription { get; init; }
    public string Discipline { get; init; }
    public string McStatus { get; init; }
    public string? Phase { get; init; }
    public bool IsVoided { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdated { get; init; }
}