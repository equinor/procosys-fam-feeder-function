using Dapper;
using Equinor.ProCoSys.PcsServiceBus.Queries;

namespace Infrastructure.CompletionQueries;

public static class PunchHistoryQuery
{
    public static (string queryString, DynamicParameters parameters) GetQuery(string plant, string? extraClause = null)
    {
        QueryHelper.DetectFaultyPlantInput(plant);
        long? discard = null;
        var whereClause = QueryHelper.CreateWhereClause(discard, plant, "plh", "irrelevant");

        if (extraClause != null)
        {
            whereClause.clause += extraClause;
        }

        var query = @$"select
            plh.projectschema as Plant,
            plh.procosys_guid as ProCoSysGuid,
            pl.procosys_guid as PunchItemGuid,
            plh.field_name as FieldName,
            plh.oldvalue as OldValue,
            plh.oldvaluelong as OldValueLong,
            plh.newvalue as NewValue,
            plh.newvaluelong as NewValueLong,
            plh.changedat as ChangedAt,
            plh.changedby as ChangedBy
        from punchlistitem_changehistory plh
            join punchlistitem pl on pl.punchlistitem_id = plh.punchlistitem_id
            join tagcheck tc on tc.tagcheck_id = pl.tagcheck_id
            join TagFormularType tft ON tc.TagFormularType_Id = tft.TagFormularType_Id
            join FormularType ft ON tft.FormularType_Id = ft.FormularType_Id
            join Tag t on tft.Tag_Id = t.tag_id
            join Project p on p.project_id = t.project_id and p.isvoided = 'N'
            join projectschema ps on ps.projectschema = plh.projectschema and ps.isvoided = 'N'
        {whereClause.clause}
        ";

        return (query, whereClause.parameters);
    }
}