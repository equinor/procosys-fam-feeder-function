using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class WorkOrderMaterial : IWorkOrderMaterialEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public string WoNo { get; init; }
    public long WoId { get; init; }
    public Guid WoGuid { get; init; }
    public string ItemNo { get; init; }
    public string? TagNo { get; init; }
    public long? TagId { get; init; }
    public Guid? TagGuid { get; init; }
    public string TagRegisterCode { get; init; }
    public long? StockId { get; init; }
    public double? Quantity { get; init; }
    public string? UnitName { get; init; }
    public string? UnitDescription { get; init; }
    public string? AdditionalInformation { get; init; }
    public DateOnly? RequiredDate { get; init; }
    public DateOnly? EstimatedAvailableDate { get; init; }
    public bool? Available { get; init; }
    public string? MaterialStatus { get; init; }
    public string? StockLocation { get; init; }
    public DateTime LastUpdated { get; init; }
}