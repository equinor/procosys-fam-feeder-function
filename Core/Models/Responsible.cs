using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class Responsible :IResponsibleEventV1

{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public long ResponsibleId { get; init; }
    public string Code { get; init; }
    public string ResponsibleGroup { get; init; }
    public string? Description { get; init; }
    public bool IsVoided { get; init; }
    public DateTime LastUpdated { get; init; }
}