namespace Infrastructure.Repositories.Queries;

internal class SwcrSignatureQuery
{

    internal static string GetQuery(string schema)
    {
        return @$"select
    '{{""Plant"" : ""' || sign.projectschema ||
    '"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') ||
    '"", ""ProjectName"" : ""' || p.NAME ||
    '"", ""SWCRNO"" : ""' || s.SWCRNO ||
    '"", ""SignatureRoleCode"" : ""' || regexp_replace(sr.code, '([""\])', '\\\1') ||
    '"", ""SignatureRoleDescription"" : ""' || regexp_replace(sr.description, '([""\])', '\\\1') ||
    '"", ""Sequence"" : ""' || sign.ranking ||
    '"", ""SignedByAzureOid"" : ""' || p.azure_oid ||
    '"", ""FunctionalRoleCode"" : ""' || regexp_replace(fr.code, '([""\])', '\\\1') ||
    '"", ""FunctionalRoleDescription"" : ""' ||regexp_replace(fr.description, '([""\])', '\\\1') ||
    '"", ""SignedDate"" : ""' || TO_CHAR(sign.signedat, 'YYYY-MM-DD hh:mm:ss') ||
    '"", ""LastUpdated"" : ""' || TO_CHAR(sign.last_updated, 'YYYY-MM-DD hh:mm:ss') ||
    '""}}' as message
    from swcrsignature  sign
        join  swcr s on  s.swcr_id = sign.swcr_id
        join projectschema ps ON ps.projectschema = sign.projectschema
        join project p ON p.project_id = s.project_id
        join library sr ON sr.library_id = sign.signaturerole_id
        left join person p ON p.person_id = sign.signedby_id
        left join library fr On fr.library_id = sign.functionalrole_id
    where sign.projectschema = '{schema}'";
    }
}
