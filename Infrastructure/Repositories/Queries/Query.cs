namespace Infrastructure.Repositories.Queries
{
    internal class Query
    {
        internal static string SqlQuery = @"SELECT
        '{""Plant"" : ""' || NEW.projectschema
        || '"",""QueryId"" : ""'|| do.DOCUMENT_ID
        || '"", ""QueryNo"" : ""'|| do.DOCUMENTNO
        || '"", ""Description"" : ""'|| regexp_replace(do.TITLE , '([""\])', '\\\1')
        || '"", ""Discipline"" : ""'|| dlib.CODE
        || '"", ""QueryType"" : ""'|| qtlib.CODE
        || '"", ""CostImpactId"" : ""'||  ci.CODE
        || '"", ""Consequence"" : ""'||  regexp_replace(NEW.CONSEQUENCE , '([""\])', '\\\1')
        || '"", ""ProposedSolution"" : ""'|| regexp_replace(NEW.PROPOSEDSOLUTION , '([""\])', '\\\1')
        || '"", ""EngineeringReply"" : ""'||  regexp_replace(NEW.Engineeringreply, '([""\])', '\\\1')
        || '"", ""Milestone"" :""'|| (select code
                                       FROM procosys.library
                                       WHERE library_id =
                                         (SELECT library_id
                                          FROM procosys.elementfield fi_ex
                                          WHERE fi_ex.ELEMENT_ID = NEW.DOCUMENT_ID
                                          AND EXISTS
                                            (SELECT 1
                                            FROM procosys.field f
                                            WHERE f.columnname = 'QUERY_SM'
                                            AND f.field_id = fi_ex.field_id)))
        || '"", ""ScheduleImpact"" : ""'||  decode(NEW.SCHEDULEIMPACT,'Y', 'true', 'N', 'false')
        || '"", ""PossibleWarrentyClaim"" : ""'||  decode(NEW.POSSIBLEWARRENTYCLAIM,'Y', 'true', 'N', 'false')
        || '"", ""IsVoided"" : ""' || decode(e.IsVoided,'Y', 'true', 'N', 'false')
        || '"",""RequiredDate"" : ""'||  TO_CHAR(new.REQUIREDREPLYDATE, 'YYYY-MM-DD hh:mm:ss')
        || '"", ""CreatedAt"" :""'||  TO_CHAR(e.CREATEDAT, 'YYYY-MM-DD hh:mm:ss')
        || '"", ""LastUpdated"" : ""'|| TO_CHAR(new.last_updated, 'YYYY-MM-DD hh:mm:ss')
        || '""}'  as message
        FROM query NEW
            JOIN document DO ON do.DOCUMENT_ID = new.DOCUMENT_ID
            JOIN element e on e.element_id = do.document_id
            JOIN library dlib on dlib.library_id = do.discipline_id
            JOIN library qtlib on qtlib.library_id = new.QUERYTYPE_ID
            JOIN library ci ON ci.library_id = NEW.COSTIMPACT_ID
         WHERE NEW.projectschema = 'PCS$JOHAN_CASTBERG'";
    }
}