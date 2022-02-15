namespace Infrastructure.Repositories.Queries
{
    internal class QuerySignature
    {
        internal static string SqlQuery = @" SELECT
         '{""Plant"" : ""' || NEW.projectschema
        ||'"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1')
        ||'"", ""ProjectName"" : ""' || p.NAME
        || '"", ""QueryId"": ""'|| do.document_id
        || '"", ""QueryNo"": ""'|| do.documentno
        ||'"", ""SignatureRoleCode"": ""' || regexp_replace(sr.code, '([""\])', '\\\1')
        ||'"", ""SignatureRoleDescription"": ""' || regexp_replace(sr.description, '([""\])', '\\\1')
        ||'"", ""FunctionalRoleCode"": ""' || regexp_replace(fr.code, '([""\])', '\\\1')
        ||'"", ""Sequence"": ""' || NEW.ranking
        ||'"", ""SignedByAzureOid"": ""' || p.azure_oid
        ||'"", ""FunctionalRoleDescription"": ""' ||regexp_replace(fr.description, '([""\])', '\\\1')
        ||'"", ""SignedDate"": ""' || TO_CHAR(NEW.signedat, 'YYYY-MM-DD hh:mm:ss')
        ||'"", ""LastUpdated"": ""' || TO_CHAR(NEW.last_updated, 'YYYY-MM-DD hh:mm:ss')
        || '""}'
    FROM querysignature NEW
        JOIN document DO ON do.document_id = NEW.DOCUMENT_ID
        JOIN project p ON p.project_id = do.project_id
        JOIN projectschema ps on ps.projectschema = NEW.projectschema
        JOIN element em on em.element_id = do.document_id
        JOIN library dlib on dlib.library_id = do.discipline_id
        JOIN library sr ON sr.library_id = NEW.signaturerole_id
        LEFT JOIN person p ON p.person_id = NEW.signedby_id
        LEFT JOIN library fr On fr.library_id = NEW.functionalrole_id
     WHERE NEW.projectschema = 'PCS$JOHAN_CASTBERG'";
    }
}