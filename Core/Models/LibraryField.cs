using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618

public class LibraryField : ILibraryFieldEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid LibraryGuid { get; init; }
    public string? LibraryType { get; init; }
    public string Code { get; init; }
    public string ColumnName { get; init; }
    public string? ColumnType { get; init; }
    public string? StringValue { get; init; }
    public DateOnly? DateValue { get; init; }
    public double? NumberValue { get; init; }
    public Guid? LibraryValueGuid { get; init; }
    public DateTime LastUpdated { get; init; }
}