namespace Infrastructure.Repositories.Queries;

internal class PunchListItemQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
      '{{""Plant"" : ""' || pl.projectschema ||                  
      '"", ""ProjectName"" : ""' || p.name ||
      '"", ""LastUpdated"" : ""' || TO_CHAR(pl.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:ss') ||                                           
      '"", ""PunchItemNo"" : ""' || pl.PunchListItem_Id ||
      '"", ""Description"" : ""' || regexp_replace(pl.Description, '([""\])', '\\\1') ||
      '"", ""ChecklistId"" : ""' || pl.tagcheck_id ||
      '"", ""TagNo"" : ""' || regexp_replace(t.tagno, '([""\])', '\\\1') ||
      '"", ""RegisterCode"" : ""' ||regexp_replace(reg.code, '([""\])', '\\\1') ||
      '"", ""ResponsibleCode"" : ""' || l_resp_code  ||         
      '"", ""ResponsibleDescription"" : ""' || l_resp_descr  ||                         
      '"", ""FormType"" : ""' || l_form_type ||
      '"", ""Category"" : ""' || l_category ||
      '"", ""RaisedByOrg"" : ""' || l_raised_by_org ||
      '"", ""ClearingByOrg"" : ""' || l_clear_by_org ||
      '"", ""DueDate"" : ""' || TO_CHAR(pl.duedate, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""PunchListSorting"" : ""' || l_pl_sorting ||
      '"", ""PunchListType"" : ""' || l_pl_type ||
      '"", ""PunchPriority"" : ""' || l_pl_pri ||
      '"", ""Estimate"" : ""' || pl.estimate ||
      '"", ""OriginalWoNo"" : ""' || l_original_wo_no ||
      '"", ""WoNo"" : ""' || wo.wono ||
      '"", ""SWCRNo"" : ""' || l_swcr_no ||
      '"", ""DocumentNo"" : ""' || l_document_no ||
      '"", ""ExternalItemNo"" : ""' ||  regexp_replace(pl.external_itemno, '([""\])', '\\\1') ||
      '"", ""MaterialRequired"" : ' || decode(pl.ismaterialrequired,'Y', 'true', 'false') ||
      ', ""IsVoided"" : ' || decode(pl.isVoided,'Y', 'true', 'false') ||
      ', ""MaterialETA"" : ""' || TO_CHAR(pl.material_eta, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""MaterialExternalNo"" : ""' || regexp_replace(pl.materialno, '([""\])', '\\\1') ||
      '"", ""ClearedAt"" : ""' || TO_CHAR(pl.clearedat, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""RejectedAt"" : ""' || TO_CHAR(pl.rejectedat, 'yyyy-mm-dd hh24:mi:ss') ||
      '"", ""VerifiedAt"" : ""' || TO_CHAR(pl.verifiedat, 'yyyy-mm-dd hh24:mi:ss') ||
      '""}}' as message 
       from punchlistitem pl
           join tagcheck tc on tc.tagcheck_id = pl.tagcheck_id
           left join Responsible r ON tc.Responsible_id = r.Responsible_Id
           left join TagFormularType tft ON tc.TagFormularType_Id = tft.TagFormularType_Id
           left join FormularType ft ON tft.FormularType_Id = ft.FormularType_Id
           left join Tag t on tft.Tag_Id = t.tag_id
           left join library reg on reg.library_id = tag.register_id
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