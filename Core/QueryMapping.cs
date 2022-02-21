using Equinor.ProCoSys.PcsServiceBus;

namespace Core;

public static class QueryMapping
{
    public static (PcsTopic, string) CheckList { get; } = (PcsTopic.Checklist, "CheckListId");
    public static (PcsTopic, string) CommPkg { get; } = (PcsTopic.CommPkg, "CommPkgNo");
    public static (PcsTopic, string) McPkg { get; } = (PcsTopic.McPkg, "McPkgNo");
    public static (PcsTopic, string) Milesstone { get; } = (PcsTopic.Milestone, "Code");
    public static (PcsTopic, string) Project { get; } = (PcsTopic.Project, "Project");

    public static (PcsTopic, string) PunchListItem { get; } = (PcsTopic.PunchListItem, "PunchItemNo");

    //public static (PcsTopic, string) SWCR { get; } = { PcsTopic.Swcr, "SWCRNO" };
    //public static (PcsTopic, string) SWCRSignature { get; } = { "SWCRSignature", "SWCRNO" };
    public static (PcsTopic, string) Tag { get; } = (PcsTopic.Tag, "TagNo");
    public static (PcsTopic, string) WorkOrder { get; } = (PcsTopic.WorkOrder, "WoNo");

    public static (PcsTopic, string) WorkOrderChecklist { get; } = (PcsTopic.WoChecklist, "WoNo");
    //public static (PcsTopic, string) Query { get; } =  ( PcsTopic.Query, "QueryId" );
    //public static (PcsTopic, string) QuerySignature { get; } = { "QuerySignature", "QueryNo" };
}