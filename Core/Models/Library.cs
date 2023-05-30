using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class Library : ILibraryEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public long LibraryId { get; init; }
    public int? ParentId { get; init; }
    public Guid? ParentGuid { get; init; }
    public string Code { get; init; }
    public string? Description { get; init; }
    public bool IsVoided { get; init; }
    public string Type { get; init; }
    public DateTime LastUpdated { get; init; }
}