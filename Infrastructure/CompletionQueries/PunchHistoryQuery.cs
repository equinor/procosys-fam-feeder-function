using Dapper;
using Equinor.ProCoSys.PcsServiceBus.Queries;

namespace Infrastructure.CompletionQueries;

public static class PunchHistoryQuery
{
    public static (string queryString, DynamicParameters parameters) GetQuery(string plant)
    {
        QueryHelper.DetectFaultyPlantInput(plant);
        long? discard = null;
        var whereClause = QueryHelper.CreateWhereClause(discard, plant, "pc", "irrelevant");

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
        {whereClause.clause}
        ";

        return (query, whereClause.parameters);
    }
}