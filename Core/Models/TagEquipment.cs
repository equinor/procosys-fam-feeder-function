using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class TagEquipment : ITagEquipmentEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ManufacturerName { get; init; }
    public string? ModelNo { get; init; }
    public string? VariantNo { get; init; }
    public string? EqHubId { get; init; }
    public string? SemiId { get; init; }
    public string? ModelName { get; init; }
    public string? ModelSubName { get; init; }
    public string? ModelSubSubName { get; init; }
    public Guid TagGuid { get; init; }
    public string TagNo { get; init; }
    public string ProjectName { get; init; }
    public DateTime LastUpdated { get; init; }
}