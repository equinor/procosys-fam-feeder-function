namespace Core.Models;

public class PunchItemComment
{
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public Guid PunchItemGuid { get; init; }
    public string Text { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdated { get; init; }
    public Guid? CreatedByGuid { get; init; }
}