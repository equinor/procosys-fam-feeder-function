using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;

#pragma warning disable CS8618
public class NotificationWorkOrder : INotificationWorkOrderEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public Guid ProjectGuid { get; init; }
    public Guid NotificationGuid { get; init; }
    public Guid WorkOrderGuid { get; init; }
    public DateTime LastUpdated { get; init; }
}
