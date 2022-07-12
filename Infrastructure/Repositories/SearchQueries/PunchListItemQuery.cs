namespace Infrastructure.Repositories.SearchQueries;

internal class PunchListItemQuery
{
    internal static string GetQueryWithProjectNames(string schema)
    {
        return @$"select
      '{{""Plant"" : ""' || pl.projectschema ||                  
      '"", ""ProjectName"" : ""' || p.name ||
      '"", ""LastUpdated"" : ""' || TO_CHAR(pl.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:ss') ||                                           
      '"", ""PunchItemNo"" : ""' || pl.PunchListItem_Id ||
      '"", ""Description"" : ""' || regexp_replace(pl.Description, '([""\])', '\\\1') ||
      '"", ""ChecklistId"" : ""' || pl.tagcheck_id ||
      '"", ""Category"" : ""' || regexp_replace(cat.code, '([""\])', '\\\1') ||
      '"", ""RaisedByOrg"" : ""' || regexp_replace(raised.code, '([""\])', '\\\1') ||
      '"", ""ClearingByOrg"" : ""' || regexp_replace(cleared.code, '([""\])', '\\\1') ||
      '"", ""DueDate"" : ""' || TO_CHAR(pl.duedate, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""PunchListSorting"" : ""' || regexp_replace(plsorting.code, '([""\])', '\\\1') ||
      '"", ""PunchListType"" : ""' || regexp_replace(pltype.code, '([""\])', '\\\1') ||
      '"", ""PunchPriority"" : ""' || regexp_replace(plpri.code, '([""\])', '\\\1') ||
      '"", ""Estimate"" : ""' || pl.estimate ||
      '"", ""OriginalWoNo"" : ""' || regexp_replace(orgwo.wono, '([""\])', '\\\1') ||
      '"", ""WoNo"" : ""' || regexp_replace(wo.wono, '([""\])', '\\\1') ||
      '"", ""SWCRNo"" : ""' || swcr.swcrno ||
      '"", ""DocumentNo"" : ""' || regexp_replace(doc.documentno, '([""\])', '\\\1') ||
      '"", ""ExternalItemNo"" : ""' ||  regexp_replace(pl.external_itemno, '([""\])', '\\\1') ||
      '"", ""MaterialRequired"" : ' || decode(pl.ismaterialrequired,'Y', 'true', 'false') ||
      ', ""IsVoided"" : ' || decode(pl.isVoided,'Y', 'true', 'false') ||
      ', ""MaterialETA"" : ""' || TO_CHAR(pl.material_eta, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""MaterialExternalNo"" : ""' || regexp_replace(pl.materialno, '([""\])', '\\\1') ||
      '"", ""ClearedAt"" : ""' || TO_CHAR(pl.clearedat, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""RejectedAt"" : ""' || TO_CHAR(pl.rejectedat, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""VerifiedAt"" : ""' || TO_CHAR(pl.verifiedat, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""ProjectNames"" : [' || (SELECT substr((select SYS_CONNECT_BY_PATH('""' || regexp_replace(NAME, '([""\])', '\\\1') || '""' , ', ') ProjectPath from PROJECT WHERE PARENT_PROJECT_ID IS NULL start with PROJECT_ID = p.project_id connect by prior PARENT_PROJECT_ID = PROJECT_ID),2) FROM DUAL) ||
      ']}}' as message
       from punchlistitem pl
           join tagcheck tc on tc.tagcheck_id = pl.tagcheck_id
           left join Responsible r ON tc.Responsible_id = r.Responsible_Id
           left join TagFormularType tft ON tc.TagFormularType_Id = tft.TagFormularType_Id
           left join FormularType ft ON tft.FormularType_Id = ft.FormularType_Id
           left join Tag t on tft.Tag_Id = t.tag_id
           left join library reg on reg.library_id = t.register_id
           left join Project p on p.project_id=t.project_id
           left join wo on wo.wo_id = pl.wo_id
           left join library cat on cat.library_id = pl.Status_Id
           left join library raised on raised.library_id = pl.raisedbyorg_id
           left join library cleared on cleared.library_id = pl.clearedbyorg_id
           left join library pltype on pltype.library_id = pl.punchlisttype_id
           left join library plsorting on plsorting.library_id = pl.PUNCHLISTSORTING_ID
           left join library plpri on plpri.library_id = pl.priority_id
           left join wo orgwo on orgwo.wo_id = pl.originalwo_id
           left join swcr on swcr.swcr_id = pl.swcr_id
           left join document doc on doc.document_id = pl.drawing_id
       where tc.projectschema = '{schema}'";
    }
}