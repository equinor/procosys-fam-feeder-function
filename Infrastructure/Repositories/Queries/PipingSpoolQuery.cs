namespace Infrastructure.Repositories.Queries;

public class PipingSpoolQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
            '{{""Plant"" : ""' || ps.projectschema || '"",
            ""Project"" : ""' ||  regexp_replace(p.name, '([""\])', '\\\1') || '"",
            ""PipingSpoolId"" : ""' || ps.pipingspool_id || '"",
            ""PipingRevisionId"" : ""' || ps.pipingrevision_id || '"",
            ""Revision"" : ""' || pr.testrevisionno || '"",
            ""McPkgNo"" : ""' || regexp_replace(m.mcpkgno, '([""\])', '\\\1') || '"",
            ""ISODrawing"" : ""' || regexp_replace(iso.documentno, '([""\])', '\\\1') || '"",
            ""Spool"" : ""' || regexp_replace(ps.spool, '([""\])', '\\\1') || '"",
            ""LineNo"" : ""' || regexp_replace(t.tagno, '([""\])', '\\\1') || '"",
            ""N2HeTest"" : ""' || ps.n2_he_test || '"",
            ""AlternativeTest"" : ""' || ps.Alternativetest || '"",
            ""AlternativeTestNoOfWelds"" : ""' || ps.NOOFWELDSAT || '"",
            ""Installed"" : ' || decode(ps.installed,'Y', 'true', 'false') || ',
            ""Welded"" : ""' || decode(ps.tackwelded,'Y', 'true', 'N', 'false') || '"",
            ""WeldedDate"" : ""' || TO_CHAR(ps.weldeddate, 'yyyy-mm-dd hh24:mi:ss') || '"",
            ""PressureTested"" : ""' || decode(ps.welded ,'Y', 'true', 'N', 'false') || '"",
            ""NDE"" : ""' || decode(ps.nde,'Y','true', 'N', 'false') || '"",
            ""Primed"" : ""' || decode(ps.primed,'Y', 'true', 'N', 'false') || '"",
            ""Painted"" : ""' || decode(ps.painted,'Y', 'true', 'N', 'false') || '"",
            ""LastUpdated"" : ""' || TO_CHAR(ps.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:ss') || '""
            }}' as message
            from pipingspool ps
                join pipingrevision pr on pr.pipingrevision_id = ps.pipingrevision_id
                join mcpkg m on m.mcpkg_id = pr.mcpkg_id
                join project p on p.project_id=m.project_id
                left join document iso on iso.document_id = ps.document_id
                join tag t on t.tag_id = ps.tag_id
            where ps.projectschema = '{schema}'";
    }
}