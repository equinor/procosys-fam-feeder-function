using Dapper;
using Equinor.ProCoSys.PcsServiceBus.Queries;

namespace Infrastructure.CompletionQueries;

public static class LibraryForPunchQuery
{
    public static (string query, DynamicParameters parameters) GetQuery(long? libraryId, string? plant = null)
    {
        QueryHelper.DetectFaultyPlantInput(plant);
        var whereClause = QueryHelper.CreateWhereClause(libraryId, plant, "l", "library_id");

        var query = @$"select
            l.projectschema as Plant,
            l.procosys_guid as ProCoSysGuid,
            l.library_id as LibraryId,
            l.parent_id as ParentId,
            lp.procosys_guid as ParentGuid,
            l.code as Code,
            l.description as Description,
            l.isVoided as IsVoided,
            l.librarytype as Type,
            l.LAST_UPDATED as LastUpdated
        from library l
            left join library lp on l.parent_id = lp.library_id
        {whereClause.clause}
        and l.librarytype in ('COMPLETION_ORGANIZATION','PUNCHLIST_SORTING', 'PUNCHLIST_TYPE', 'COMM_PRIORITY')
        ";

        return (query, whereClause.parameters);
        
    }
}

