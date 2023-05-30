using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class WorkOrderMilestone : IWorkOrderMilestoneEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public long WoId { get; init; }
    public Guid? WoGuid { get; init; }
    public string WoNo { get; init; }
    public string Code { get; init; }
    public DateOnly? MilestoneDate { get; init; }
    public string? SignedByAzureOid { get; init; }
    public DateTime LastUpdated { get; init; }
}