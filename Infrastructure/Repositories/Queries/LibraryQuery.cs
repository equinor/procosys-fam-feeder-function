

namespace Infrastructure.Repositories.Queries;

internal class LibraryQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
            '{{""Plant"" : ""' || l.projectschema || 
            '"", ""LibraryId"" : ""' || l.library_id || 
            '"", ""Code"" : ""' || regexp_replace(l.code, '([""\])', '\\\1') || 
            '"", ""Description"" : ""' || regexp_replace(l.description, '([""\])', '\\\1') || 
            '"", ""IsVoided"" : ""' || || decode(l.isVoided,'Y', 'true', 'N', 'false') ||
            '"", ""Type"" : ""' || REPLACE(REPLACE(l.librarytype,'\','\\'),'""','\""') ||
            '"", ""LastUpdated"" : ""' || TO_CHAR(l.LAST_UPDATED, 'YYYY-MM-DD hh:mm:ss') ||
            '""}}'  as message
            from library l
            where l.projectschema = '{schema}'";
    }

}