using Dapper;
using Equinor.ProCoSys.PcsServiceBus.Queries;

namespace Infrastructure.CompletionQueries;

public static class PunchCommentsQuery
{
    public static (string query, DynamicParameters parameters) GetQuery(string? plant = null, string? extraClause = null)
    {
        QueryHelper.DetectFaultyPlantInput(plant);
        long? discard = null;
        var whereClause = QueryHelper.CreateWhereClause(discard, plant, "pc", "irrelevant");
        
        if (extraClause != null)
        {
            whereClause.clause += extraClause;
        }

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
            join tagcheck tc on tc.tagcheck_id = pli.tagcheck_id
            join TagFormularType tft ON tc.TagFormularType_Id = tft.TagFormularType_Id
            join FormularType ft ON tft.FormularType_Id = ft.FormularType_Id
            join Tag t on tft.Tag_Id = t.tag_id
            join Project p on p.project_id = t.project_id and p.isvoided = 'N'
            join projectschema ps on ps.projectschema = pc.projectschema and ps.isvoided = 'N'
            join person p on p.person_id = pc.createdby_id
        {whereClause.clause}
        ";

        return (query, whereClause.parameters);
        
    }
}