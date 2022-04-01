namespace Infrastructure.Repositories.Queries;

internal class MilestonesQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
            '{{""Plant"" : ""' || e.projectschema || 
            '"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') ||
            '"", ""ProjectName"" : ""' || p.name ||  
            '"", ""CommPkgNo"" : ""' || c.COMMPKGNO ||
            '"", ""McPkgNo"" : ""' || m.MCPKGNO ||
            '"", ""Code"" : ""' || milestone.code || 
            '"", ""ActualDate"" : ""' || TO_CHAR(e.actualdate, 'yyyy-mm-dd hh24:mi:sss') ||
            '"", ""PlannedDate"" : ""' || TO_CHAR(e.planneddate, 'yyyy-mm-dd hh24:mi:sss') ||   
            '"", ""IsSent"" : ""' || decode(cert.issent,'Y', 'true', 'N', 'false') ||  
            '"", ""IsAccepted"" : ""' || decode(cert.isaccepted,'Y', 'true', 'N', 'false') ||
            '"", ""IsRejected"" : ""' || decode(cert.isrejected,'Y', 'true', 'N', 'false') || 
            '""}}' as message
            from completionmilestonedate e
                join projectschema ps on ps.projectschema = e.projectschema
                join library milestone on milestone.library_id = e.milestone_id
                left join commpkg c on c.commpkg_id = e.element_id
                left join mcpkg m on m.mcpkg_id = e.element_id
                left join project p on p.project_id = COALESCE(c.project_id,m.project_id)
                left join V$Certificate cert on cert.certificate_id = e.certificate_id
            where e.projectschema = '{schema}'";
    }
}