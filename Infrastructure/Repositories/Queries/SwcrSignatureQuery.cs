namespace Infrastructure.Repositories.Queries;

internal class SwcrSignatureQuery
{

    internal static string Query = @" SELECT
    '{""Plant"" : ""' || sign.projectschema ||
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
    '""}' as message
    from swcrsignature  sign
        join  swcr s on  s.swcr_id = sign.swcr_id
        JOIN projectschema ps ON ps.projectschema = sign.projectschema
        JOIN project p ON p.project_id = s.project_id
        JOIN library sr ON sr.library_id = sign.signaturerole_id
        LEFT JOIN person p ON p.person_id = sign.signedby_id
        LEFT JOIN library fr On fr.library_id = sign.functionalrole_id
    WHERE sign.projectschema = 'PCS$JOHAN_CASTBERG'";
}
