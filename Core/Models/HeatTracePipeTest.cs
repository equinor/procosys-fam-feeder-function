using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618

public class HeatTracePipeTest : IHeatTracePipeTestEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid TagGuid { get; init; }
    public string Name { get; init; }
    public DateTime LastUpdated { get; init; }
}