namespace Infrastructure.Repositories.Queries;

internal class ChecklistQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
        '{{""Plant"" : ""' || tc.projectschema || 
        '"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') ||
        '"", ""ProjectName"" : ""' || p.name ||
        '"", ""TagNo"" : ""' ||  regexp_replace(t.tagno, '([""\])', '\\\1') ||
        '"", ""ChecklistId"" : ""' || tc.tagcheck_id ||
        '"", ""TagCategory"" : ""' || reg.code ||
        '"", ""FormularType"" : ""' || ft.formulartype ||
        '"", ""FormularGroup"" : ""' || ft.formulargroup ||
        '"", ""FormularDiscipline"" : ""' || mccr_disc.code ||
        '"", ""Revision"" : ""' || pir.testrevisionno ||
        '"", ""Responsible"" : ""' || r.code ||
        '"", ""Status"" : ""' || status.code ||
        '"", ""UpdatedAt"" : ""' || TO_CHAR(tc.updatedat, 'YYYY-MM-DD hh:mm:ss') ||
        '"", ""CreatedAt"" : ""' || TO_CHAR(tc.createdat, 'YYYY-MM-DD hh:mm:ss') ||
        '"", ""SignedAt"" : ""' || TO_CHAR(tc.signedat, 'YYYY-MM-DD hh:mm:ss') ||
        '"", ""VerifiedAt"" : ""' || TO_CHAR(tc.verifiedat, 'YYYY-MM-DD hh:mm:ss') ||
        '""}}' as message
        from tagcheck tc
            join projectschema ps on ps.projectschema = tc.projectschema
            join tagformulartype tft on tft.tagformulartype_id = tc.tagformulartype_id
            join tag t on t.tag_id = tft.tag_id
            join formulartype ft on ft.formulartype_id = tft.formulartype_id
            join library mccr_disc on mccr_disc.library_id = ft.discipline_id
            left join pipingrevision pir on pir.pipingrevision_id = tft.pipingrevision_id
            join project p on p.project_id = t.project_id
            left join library reg on reg.library_id = t.register_id
            left join responsible r on r.responsible_id = tc.responsible_id
            left join library status on status.library_id = tc.status_id
        where tc.projectschema = '{schema}'";
    }
}