using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;

public class Notification : INotificationEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public long NotificationId { get; init; }
    public string NotificationNo { get; init; }
    public string? NotificationType { get; init; }
    public string? DocumentType { get; init; }
    public string? Title { get; init; }
    public string? ResponsibleContractor { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdated { get; init; }
}
