using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class CommPkgTask : ICommPkgTaskEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public string ProjectName { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid TaskGuid { get; init; }
    public Guid CommPkgGuid { get; init; }
    public string CommPkgNo { get; init; }
    public DateTime LastUpdated { get; init; }
}