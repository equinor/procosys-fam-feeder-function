using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class LoopContent : ILoopContentEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public int LoopTagId { get; init; }
    public Guid LoopTagGuid { get; init; }
    public int TagId { get; init; }
    public Guid TagGuid { get; init; }
    public string RegisterCode { get; init; }
    public DateTime LastUpdated { get; init; }
}