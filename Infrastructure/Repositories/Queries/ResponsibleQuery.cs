

namespace Infrastructure.Repositories.Queries;

public class ResponsibleQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
            '{{""Plant"" : ""' || p.projectschema || 
            '"", ""ResponsibleId"" : ""' || r.responsible_id || 
            '"", ""Code"" : ""' || regexp_replace(r.code, '([""\])', '\\\1') ||
            '"", ""ResponsibleGroup"" : ""' || regexp_replace(r.responsiblegroup, '([""\])', '\\\1') ||
            '"", ""Description"" : ""' || regexp_replace(r.description, '([""\])', '\\\1') || 
            '"", ""IsVoided"" : ""' || || decode(r.isVoided,'Y', 'true', 'N', 'false') ||
            '"", ""LastUpdated"" : ""' || TO_CHAR(r.LAST_UPDATED, 'YYYY-MM-DD hh:mm:ss') ||
            '""}}'  as message
            from responsible r
            where l.projectschema = '{schema}'";
    }
}