using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using JetBrains.Annotations;

namespace Core.Models;
#pragma warning disable CS8618

[UsedImplicitly]
public class HeatTrace : IHeatTraceEventV1
{
    public string EventType => "HeatTraceEvent";
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public long HeatTraceId { get; init; }
    public long CableId { get; init; }
    public Guid CableGuid { get; init; }
    public string CableNo { get; init; }
    public long TagId { get; init; }
    public Guid TagGuid { get; init; }
    public string TagNo { get; init; }
    public string? SpoolNo { get; init; }
    public DateTime LastUpdated { get; init; }
}