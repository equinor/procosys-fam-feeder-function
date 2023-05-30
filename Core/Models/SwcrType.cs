using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class SwcrType : ISwcrTypeEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid LibraryGuid { get; init; }
    public Guid SwcrGuid { get; init; }
    public string Code { get; init; }
    public DateTime LastUpdated { get; init; }
}