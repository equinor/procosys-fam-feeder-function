using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;

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
    public string EventType { get; }
}

