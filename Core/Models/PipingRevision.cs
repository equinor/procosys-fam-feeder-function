using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class PipingRevision : IPipingRevisionEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public long PipingRevisionId { get; init; }
    public int Revision { get; init; }
    public string McPkgNo { get; init; }
    public Guid McPkgNoGuid { get; init; }
    public string ProjectName { get; init; }
    public double? MaxDesignPressure { get; init; }
    public double? MaxTestPressure { get; init; }
    public string? Comments { get; init; }
    public string? TestISODocumentNo { get; init; }
    public Guid? TestISODocumentGuid { get; init; }
    public int? TestISORevision { get; init; }
    public string? PurchaseOrderNo { get; init; }
    public string? CallOffNo { get; init; }
    public Guid? CallOffGuid { get; init; }
    public DateTime LastUpdated { get; init; }
}