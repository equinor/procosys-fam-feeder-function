using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618

public class CommPkgQuery : ICommPkgQueryEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public long CommPkgId { get; init; }
    public Guid CommPkgGuid { get; init; }
    public string CommPkgNo { get; init; }
    public long DocumentId { get; init; }
    public string QueryNo { get; init; }
    public Guid QueryGuid { get; init; }
    public DateTime LastUpdated { get; init; }
}