using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class Project : IProjectEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public bool IsClosed { get; init; }
    public string? Description { get; init; }
    public DateTime LastUpdated { get; init; }
}