namespace Infrastructure.Repositories.Queries;

internal class WorkOrderChecklistsQuery
{
    internal static string Query =  @"SELECT
    '{""Plant"" : ""' || wotc.projectschema ||
    '"", ""ProjectName"" : ""' || p.NAME ||
    '"", ""WoNo"" : ""' || wo.WONO ||
    '"", ""TagNo"" : ""' || t.TAGNO ||
    '"", ""FormularType"" : ""' || ft.formulartype ||
    '"", ""FormularGroup"" : ""' || ft.formulargroup ||
    '"", ""Responsible"" : ""' || r.CODE ||
    '""}'
    FROM wo_tagcheck wotc
        JOIN element e on E.ELEMENT_ID = wotc.wo_ID
        JOIN wo ON wo.wo_id = wotc.wo_id
        join Tagcheck tc on tc.tagcheck_id = wotc.tagcheck_id
        JOIN projectschema ps ON ps.projectschema = wotc.projectschema
        JOIN project p ON p.project_id = wo.project_id
        JOIN tagformulartype tft ON tft.tagformulartype_id = tc.tagformulartype_id
        JOIN tag t ON t.tag_id = tft.tag_id
        JOIN formulartype ft ON ft.formulartype_id = tft.formulartype_id
        JOIN responsible r on r.responsible_id = tc.Responsible_id
    WHERE wotc.projectschema = 'PCS$JOHAN_CASTBERG'";
}