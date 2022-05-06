namespace Infrastructure.Repositories.Queries;

internal class DocumentQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"SELECT
   '{{""Plant"": ""' || D.projectschema ||
   '"", ""ProjectName"" : ""' || p.name ||
   '"", ""DocumentId"" : ""' ||D.Document_Id ||
   '"", ""DocumentNo"" : ""' || D.DocumentNo ||
   '"", ""Title"" : ""' || p.name ||
   '"", ""AcceptanceCode"" : ""' || apc.CODE ||
   '"", ""Archive"" : ""' || arc.CODE ||
   '"", ""AccessCode"" : ""' || acc.CODE ||
   '"", ""Complex"" : ""' || com.CODE ||
   '"", ""DocumentType"" : ""' || DT.CODE ||
   '"", ""DisciplineId"" : ""' || dp.CODE ||
   '"", ""DocumentCategory"" : ""' || dc.CODE ||
   '"", ""HandoverStatus"" : ""' || ho.code ||
   '"", ""RegisterType"" : ""' || RT.CODE ||
   '"", ""RevisionNo"" : ""' || D.revisionno ||
   '"", ""RevisionStatus"" : ""' || rev.code ||
   '"", ""ResponsibleContractor"" : ""' || res.code ||
   '"", ""LastUpdated"" : ""' || TO_CHAR(D.Last_Updated, 'YYYY-MM-DD hh:mm:ss') ||
   '"", ""RevisionDate"" : ""' || TO_CHAR(D.Revisiondate, 'YYYY-MM-DD hh:mm:ss') ||
   '""}}' as message
  FROM DOCUMENT D
  LEFT JOIN LIBRARY RT on RT.LIBRARY_ID = D.register_id
  LEFT JOIN LIBRARY DT on DT.LIBRARY_ID = D.documenttype_id
  LEFT JOIN LIBRARY apc on apc.LIBRARY_ID = D.acceptancecode_id
  LEFT JOIN LIBRARY dc on dc.LIBRARY_ID = D.documentcategory_id
  LEFT JOIN LIBRARY arc on arc.LIBRARY_ID = D.archive_id
  LEFT JOIN LIBRARY dp on dp.LIBRARY_ID = D.discipline_id
  LEFT JOIN LIBRARY rev on rev.LIBRARY_ID = D.revisionstatus_id
  LEFT JOIN LIBRARY ho on ho.LIBRARY_ID = D.handoverstatus_id
  LEFT JOIN LIBRARY res on res.LIBRARY_ID = D.responsiblecontractor_id
  LEFT JOIN LIBRARY acc on acc.LIBRARY_ID = D.accesscode_id
  LEFT JOIN LIBRARY com on com.LIBRARY_ID = D.complex_id
  LEFT JOIN PROJECT p on p.project_id = D.project_id
  WHERE D.projectschema ='{schema}'";
    }
}