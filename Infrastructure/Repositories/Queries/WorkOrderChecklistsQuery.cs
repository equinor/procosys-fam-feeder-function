namespace Infrastructure.Repositories.Queries;

internal class WorkOrderChecklistsQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
    '{{""Plant"" : ""' || wotc.projectschema ||
    '"", ""ProjectName"" : ""' || p.NAME ||
    '"", ""WoNo"" : ""' || wo.wono ||
    '"", ""ChecklistId"" : ""' || wotc.tagcheck_id ||
    '""}}'
    FROM wo_tagcheck wotc
        join element e on E.ELEMENT_ID = wotc.wo_ID
        join wo on wo.wo_id = wotc.wo_id
        join project p ON p.project_id = wo.project_id
    WHERE wotc.projectschema = '{schema}'";
    }
}