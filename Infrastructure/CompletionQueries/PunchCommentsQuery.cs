using Dapper;
using Equinor.ProCoSys.PcsServiceBus.Queries;

namespace Infrastructure.CompletionQueries;

public static class PunchCommentsQuery
{
    public static (string query, DynamicParameters parameters) GetQuery(string? plant = null)
    {
        QueryHelper.DetectFaultyPlantInput(plant);
        long? discard = null;
        var whereClause = QueryHelper.CreateWhereClause(discard, plant, "pc", "irrelevant");

        var query = @$"select
            pc.projectschema as Plant,
            pc.procosys_guid as ProCoSysGuid,
            pli.procosys_guid as PunchItemGuid,
            pc.text as Text,
            pc.createdAt as CreatedAt,
            p.azure_oid as CreatedByGuid,
            pc.last_updated as LastUpdated
        from punchlistitemcomment pc
            join punchlistitem pli on pli.punchlistitem_id = pc.punchlistitem_id
            left join person p on p.person_id = pc.createdby_id
        {whereClause.clause}
        ";

        return (query, whereClause.parameters);
        
    }
}