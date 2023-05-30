using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class SwcrAttachment : IAttachmentEventV1
{
    public string EventType { get; init; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid? SwcrGuid { get; init; }
    public string Title { get; init; }
    public string ClassificationCode { get; init; }
    public string Uri { get; init; }
    public string FileName { get; init; }
    public DateTime LastUpdated { get; init; }
}