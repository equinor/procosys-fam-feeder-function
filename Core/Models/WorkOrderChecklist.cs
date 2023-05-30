using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class WorkOrderChecklist : IWorkOrderChecklistEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public long ChecklistId { get; init; }
    public Guid ChecklistGuid { get; init; }
    public long WoId { get; init; }
    public Guid WoGuid { get; init; }
    public string WoNo { get; init; }
    public DateTime LastUpdated { get; init; }
}