namespace Infrastructure.Repositories.Queries;

internal class SwcrQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
    '{{""Plant"" : ""' || sw.projectschema ||
    '"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') ||
    '"", ""ProjectName"" : ""' || p.NAME ||
    '"", ""SWCRNO"" : ""' || sw.SWCRNO ||
    '"", ""SWCRId"" : ""' || sw.SWCR_ID ||
    '"", ""CommPkgNo"" : ""' || c.COMMPKGNO ||
    '"", ""Description"" : ""' || regexp_replace(sw.problemdescription, '([""\])', '\\\1') ||
    '"", ""Modification"" : ""' || regexp_replace(sw.modificationdescription, '([""\])', '\\\1') ||
    '"", ""Priority"" : ""' || pri.code ||
    '"", ""System"" : ""' || sys.code ||
    '"", ""ControlSystem"" : ""' || cs.code ||
    '"", ""Contract"" : ""' || con.code ||
    '"", ""Supplier"" : ""' || sup.code ||
    '"", ""Node"" : ""' ||  regexp_replace(n.NODENO, '([""\])', '\\\1') ||
    '"", ""CreatedAt"" : ""' || TO_CHAR(e.createdat, 'yyyy-mm-dd hh24:mi:sss')  ||
    '"", ""IsVoided"" : ""' || decode(e.IsVoided,'Y', 'true', 'N', 'false')  ||
    '"", ""LastUpdated"" : ""' || TO_CHAR(sw.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:sss')  ||
    '"", ""DueDate"" : ""' || TO_CHAR(sw.plannedfinishdate, 'yyyy-mm-dd hh24:mi:sss')  ||
    '"", ""EstimatedHours"" : ""' || sw.estimatedmhrs  ||
    '""}}' as message
    from swcr sw
        JOIN element e on  E.ELEMENT_ID = sw.swcr_ID
        JOIN projectschema ps ON ps.projectschema = sw.projectschema
        JOIN project p ON p.project_id = sw.project_id
        LEFT JOIN commpkg c ON c.commpkg_id = sw.commpkg_id
        LEFT JOIN library pri ON pri.library_id = sw.priority_id
        LEFT JOIN library sys On sys.library_id = sw.processsystem_id
        LEFT JOIN library con ON con.library_id =sw.contract_id
        LEFT JOIN library cs ON cs.library_id = sw.controlsystem_id
        LEFT JOIN library sup ON sup.library_id = sw.supplier_id
        LEFT JOIN node n ON n.node_id = sw.node_id
    WHERE sw.projectschema = '{schema}'";
    }
}