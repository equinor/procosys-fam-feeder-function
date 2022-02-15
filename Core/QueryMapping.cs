using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class QueryMapping
    {
        public static string[] CheckList { get; } = { "Checklist", "ChecklistId" };
        public static string[] CommPkg { get; } = { "CommPkg", "CommPkgNo" };
        public static string[] McPkg { get; } = { "McPkg", "McPkgNo" };
        public static string[] Milesstone { get; } = { "Milestone", "Code" };
        public static string[] Project { get; } = { "Project", "ProjectName" };
        public static string[] PunchListItem { get; } = { "PunchListItem", "PunchItemNo" };
        public static string[] SWCR { get; } = { "SWCR", "SWCRNO" };
        public static string[] SWCRSignature { get; } = { "SWCRSignature", "SWCRNO" };
        public static string[] Tag { get; } = { "Tag", "TagNo" };
        public static string[] WorkOrder { get; } = { "WorkOrder", "WoNo" };
        public static string[] WorkOrderChecklist { get; } = { "WoChecklist", "WoNo" };
        public static string[] Query { get; } = { "Query", "QueryId" };
        public static string[] QuerySignature { get; } = { "QuerySignature", "QueryNo" };
    }
}