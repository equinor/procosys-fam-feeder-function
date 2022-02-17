namespace Infrastructure.Repositories.Queries;

internal class WorkOrderChecklistsQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
    '{{""Plant"" : ""' || wotc.projectschema ||
    '"", ""ProjectName"" : ""' || p.NAME ||
    '"", ""WoNo"" : ""' || wo.WONO ||
    '"", ""TagNo"" : ""' || t.TAGNO ||
    '"", ""FormularType"" : ""' || ft.formulartype ||
    '"", ""FormularGroup"" : ""' || ft.formulargroup ||
    '"", ""Responsible"" : ""' || r.CODE ||
    '""}}'
    FROM wo_tagcheck wotc
        join element e on E.ELEMENT_ID = wotc.wo_ID
        join wo ON wo.wo_id = wotc.wo_id
        join Tagcheck tc on tc.tagcheck_id = wotc.tagcheck_id
        join projectschema ps ON ps.projectschema = wotc.projectschema
        join project p ON p.project_id = wo.project_id
        join tagformulartype tft ON tft.tagformulartype_id = tc.tagformulartype_id
        join tag t ON t.tag_id = tft.tag_id
        join formulartype ft ON ft.formulartype_id = tft.formulartype_id
        join responsible r on r.responsible_id = tc.Responsible_id
    WHERE wotc.projectschema = {schema}";
    }
}