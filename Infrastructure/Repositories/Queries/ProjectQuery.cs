namespace Infrastructure.Repositories.Queries;

internal class ProjectQuery
{
    internal static string GetQuery()
    {
        return @"select
            '{""Plant"" : ""' || p.projectschema || 
            '"", ""ProjectName"" : ""' || p.NAME || 
            '"", ""IsClosed"" : ' || (case when p.ISVOIDED = 'Y' then 'true' else 'false' end) || 
            '"", ""Description"" : ""' || REPLACE(REPLACE(p.DESCRIPTION,'\','\\'),'""','\""') || 
            '""}'  as message
            from project p";
    }
}