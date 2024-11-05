using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618

public class WorkOrderCutoff : IWorkOrderCutoffEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string PlantName { get; init; }
    public Guid WoGuid { get; init; }
    public string ProjectName { get; init; }
    public string WoNo { get; init; }
    public string? JobStatusCode { get; init; }
    public string? MaterialStatusCode { get; init; }
    public string? DisciplineCode { get; init; }
    public string? CategoryCode { get; init; }
    public string? MilestoneCode { get; init; }
    public string? SubMilestoneCode { get; init; }
    public string? HoldByCode { get; init; }
    public string? PlanActivityCode { get; init; }
    public string? ResponsibleCode { get; init; }
    public DateTime LastUpdated { get; init; }
    public int CutoffWeek { get; init; }
    public int CutoffWeekTo { get; init; }
    public DateOnly CutoffDate { get; init; }
    public DateOnly CutoffDateTo { get; init; }
    public DateOnly? PlannedStartAtDate { get; init; }
    public DateOnly? PlannedFinishedAtDate { get; init; }
    public double? ExpendedManHours { get; init; }
    public double? ManHoursEarned { get; init; }
    public double? EstimatedHours { get; init; }
    public double? ManHoursExpendedLastWeek { get; init; }
    public double? ManHoursEarnedLastWeek { get; init; }
    public double? ProjectProgress { get; init; }
}