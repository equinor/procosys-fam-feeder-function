using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using JetBrains.Annotations;

namespace Core.Models;
#pragma warning disable CS8618
[UsedImplicitly]
public class PunchItemAttachment : IHasEventType
{
    public string Plant { get; set; }
    public Guid PunchItemGuid { get; set; }
    public Guid AttachmentGuid { get; set; }
    public string ProjectName { get; set; }
    public string? FileName { get; set; }
    public string? Uri { get; set; }
    public string Title { get; set; }
    public int? FileId { get; set; }
    public Guid CreatedByGuid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; init; }
    public string LastUpdatedByUser { get; set; }
    public string EventType => "PunchItemAttachmentEvent";
}

