using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class PunchListItem : IPunchListItemEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public Guid ProjectGuid { get; init; }
    public Guid? ModifiedByGuid { get; init; }
    public DateTime LastUpdated { get; init; }
    public long PunchItemNo { get; init; }
    public string? Description { get; init; }
    public long ChecklistId { get; init; }
    public Guid ChecklistGuid { get; init; }
    public string Category { get; init; }
    public string? RaisedByOrg { get; init; }
    public Guid? RaisedByOrgGuid { get; init; }
    public string? ClearingByOrg { get; init; }
    public Guid? ClearingByOrgGuid { get; init; }
    public DateTime? DueDate { get; init; }
    public string? PunchListSorting { get; init; }
    public Guid? PunchListSortingGuid { get; init; }
    public string? PunchListType { get; init; }
    public Guid? PunchListTypeGuid { get; init; }
    public string? PunchPriority { get; init; }
    public Guid? PunchPriorityGuid { get; init; }
    public string? Estimate { get; init; }
    public string? OriginalWoNo { get; init; }
    public Guid? OriginalWoGuid { get; init; }
    public string? WoNo { get; init; }
    public Guid? WoGuid { get; init; }
    public string? SWCRNo { get; init; }
    public Guid? SWCRGuid { get; init; }
    public string? DocumentNo { get; init; }
    public Guid? DocumentGuid { get; init; }
    public string? ExternalItemNo { get; init; }
    public bool MaterialRequired { get; init; }
    public bool IsVoided { get; init; }
    public DateTime? MaterialETA { get; init; }
    public string? MaterialExternalNo { get; init; }
    public Guid? ClearedByGuid { get; init; }
    public DateTime? ClearedAt { get; init; }
    public Guid? RejectedByGuid { get; init; }
    public DateTime? RejectedAt { get; init; }
    public Guid? VerifiedByGuid { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public Guid? CreatedByGuid { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? ActionByGuid { get; init; }
}