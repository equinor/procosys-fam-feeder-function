namespace Infrastructure.Repositories.Queries;

internal class Query
{
    internal static string GetQuery(string schema)
    {
        return @$"select
           '{{""Plant"" : ""' || q.projectschema
        || '"", ""QueryId"" : ""'|| do.DOCUMENT_ID
        || '"", ""QueryNo"" : ""'|| do.DOCUMENTNO
        || '"", ""Description"" : ""'|| regexp_replace(do.TITLE , '([""\])', '\\\1')
        || '"", ""Discipline"" : ""'|| dlib.CODE
        || '"", ""QueryType"" : ""'|| qtlib.CODE
        || '"", ""CostImpactId"" : ""'||  ci.CODE
        || '"", ""Consequence"" : ""'||  regexp_replace(q.CONSEQUENCE , '([""\])', '\\\1')
        || '"", ""ProposedSolution"" : ""'|| regexp_replace(q.PROPOSEDSOLUTION , '([""\])', '\\\1')
        || '"", ""EngineeringReply"" : ""'||  regexp_replace(q.Engineeringreply, '([""\])', '\\\1')
        || '"", ""Milestone"" :""'|| (select code
                                       from procosys.library
                                       WHERE library_id =
                                         (SELECT library_id
                                          FROM procosys.elementfield fi_ex
                                          WHERE fi_ex.ELEMENT_ID = q.DOCUMENT_ID
                                          AND EXISTS
                                            (SELECT 1
                                            FROM procosys.field f
                                            WHERE f.columnname = 'QUERY_SM'
                                            AND f.field_id = fi_ex.field_id)))
        || '"", ""ScheduleImpact"" : ""'||  decode(q.SCHEDULEIMPACT,'Y', 'true', 'N', 'false')
        || '"", ""PossibleWarrentyClaim"" : ""'||  decode(q.POSSIBLEWARRENTYCLAIM,'Y', 'true', 'N', 'false')
        || '"", ""IsVoided"" : ""' || decode(e.IsVoided,'Y', 'true', 'N', 'false')
        || '"", ""RequiredDate"" : ""'||  TO_CHAR(q.REQUIREDREPLYDATE, 'YYYY-MM-DD hh:mm:ss')
        || '"", ""CreatedAt"" :""'||  TO_CHAR(e.CREATEDAT, 'YYYY-MM-DD hh:mm:ss')
        || '"", ""LastUpdated"" : ""'|| TO_CHAR(q.last_updated, 'YYYY-MM-DD hh:mm:ss')
        || '""}}'  as message
        from query q
            join document DO ON do.DOCUMENT_ID = q.DOCUMENT_ID
            join element e on e.element_id = do.document_id
            join library dlib on dlib.library_id = do.discipline_id
            join library qtlib on qtlib.library_id = q.QUERYTYPE_ID
            join library ci ON ci.library_id = q.COSTIMPACT_ID
         where q.projectschema = '{schema}'";
    }
}