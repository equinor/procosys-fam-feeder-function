using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618

public class CallOff : ICallOffEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public long CallOffId { get; init; }
    public long PackageId { get; init; }
    public string PurchaseOrderNo { get; init; }
    public bool IsCompleted { get; init; }
    public bool UseMcScope { get; init; }
    public DateTime LastUpdated { get; init; }
    public bool IsVoided { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CallOffNo { get; init; }
    public string? Description { get; init; }
    public string? ResponsibleCode { get; init; }
    public string? ContractorCode { get; init; }
    public string? SupplierCode { get; init; }
    public int? EstimatedTagCount { get; init; }
    public DateOnly? FATPlanned { get; init; }
    public DateOnly? PackagePlannedDelivery { get; init; }
    public DateOnly? PackageActualDelivery { get; init; }
    public DateOnly? PackageClosed { get; init; }
    public DateOnly? McDossierSent { get; init; }
    public DateOnly? McDossierReceived { get; init; }
}