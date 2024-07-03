using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using JetBrains.Annotations;

namespace Core.Models;
#pragma warning disable CS8618

[UsedImplicitly]
public class CommPkg : ICommPkgEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string PlantName { get; init; }
    public Guid ProjectGuid { get; init; }
    public string ProjectName { get; init; }
    public string CommPkgNo { get; init; }
    public long CommPkgId { get; init; }
    public string Description { get; init; }
    public string CommPkgStatus { get; init; }
    public bool IsVoided { get; init; }
    public DateTime LastUpdated { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? DescriptionOfWork { get; init; }
    public string? Remark { get; init; }
    public string ResponsibleCode { get; init; }
    public string? ResponsibleDescription { get; init; }
    public string? AreaCode { get; init; }
    public string? AreaDescription { get; init; }
    public string? Phase { get; init; }
    public string? CommissioningIdentifier { get; init; }
    public bool? Demolition { get; init; }
    public string? Priority1 { get; init; }
    public string? Priority2 { get; init; }
    public string? Priority3 { get; init; }
    public string? Progress { get; init; }
    public string? DCCommPkgStatus { get; init; }
}