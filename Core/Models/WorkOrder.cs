using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class WorkOrder : IWorkOrderEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public string WoNo { get; init; }
    public long WoId { get; init; }
    public string? CommPkgNo { get; init; }
    public Guid? CommPkgGuid { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? MilestoneCode { get; init; }
    public string? SubMilestoneCode { get; init; }
    public string? MilestoneDescription { get; init; }
    public string? CategoryCode { get; init; }
    public string? MaterialStatusCode { get; init; }
    public string? HoldByCode { get; init; }
    public string? DisciplineCode { get; init; }
    public string? DisciplineDescription { get; init; }
    public string? ResponsibleCode { get; init; }
    public string? ResponsibleDescription { get; init; }
    public string? AreaCode { get; init; }
    public string? AreaDescription { get; init; }
    public string? JobStatusCode { get; init; }
    public string? MaterialComments { get; init; }
    public string? ConstructionComments { get; init; }
    public string? TypeOfWorkCode { get; init; }
    public string? OnShoreOffShoreCode { get; init; }
    public string? WoTypeCode { get; init; }
    public double ProjectProgress { get; init; }
    public int Progress { get; init; }
    public string? ExpendedManHours { get; init; }
    public string? EstimatedHours { get; init; }
    public string? RemainingHours { get; init; }
    public string? WBS { get; init; }
    public DateOnly? PlannedStartAtDate { get; init; }
    public DateOnly? ActualStartAtDate { get; init; }
    public DateOnly? PlannedFinishedAtDate { get; init; }
    public DateOnly? ActualFinishedAtDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsVoided { get; init; }
    public DateTime LastUpdated { get; init; }
}