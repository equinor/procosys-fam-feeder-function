using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class Stock : IStockEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public long StockId { get; init; }
    public string StockNo { get; init; }
    public string Description { get; init; }
    public DateTime LastUpdated { get; init; }
}