using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class Query : IQueryEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public long QueryId { get; init; }
    public string QueryNo { get; init; }
    public string? Title { get; init; }
    public string? DisciplineCode { get; init; }
    public string? QueryType { get; init; }
    public string? CostImpact { get; init; }
    public string? Description { get; init; }
    public string? Consequence { get; init; }
    public string? ProposedSolution { get; init; }
    public string? EngineeringReply { get; init; }
    public string? Milestone { get; init; }
    public bool ScheduleImpact { get; init; }
    public bool PossibleWarrantyClaim { get; init; }
    public bool IsVoided { get; init; }
    public DateOnly? RequiredDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdated { get; init; }
}