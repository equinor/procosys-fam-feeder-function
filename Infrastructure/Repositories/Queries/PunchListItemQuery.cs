namespace Infrastructure.Repositories.Queries;

internal class PunchListItemQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
       '{{""Plant"" : ""' || pl.projectschema || 
       '"", ""PlantName"" : ""' || regexp_replace(ps.title, '([""\])', '\\\1') ||                          
       '"", ""ProjectName"" : ""' || regexp_replace(p.Title, '([""\])', '\\\1') || 
       '"", ""LastUpdated"" : ""' || TO_CHAR(pl.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:ss') ||                                                 
       '"", ""PunchItemNo"" : ""' || pl.PunchListItem_Id || 
       '"", ""Description"" : ""' || regexp_replace(pl.Description, '([""\])', '\\\1') || 
       '"", ""TagNo"" : ""' || regexp_replace(t.TagNo, '([""\])', '\\\1') ||
       '"", ""TagId"" : ""' || t.tag_id ||
       '"", ""RegisterCode"" : ""' || regexp_replace(reg.Code, '([""\])', '\\\1') ||
       '"", ""ResponsibleCode"" : ""' || regexp_replace(r.Code, '([""\])', '\\\1')  ||                    
       '"", ""ResponsibleDescription"" : ""' || regexp_replace(r.Description, '([""\])', '\\\1')  ||                                 
       '"", ""FormType"" : ""' || regexp_replace(ft.FormularType, '([""\])', '\\\1') ||  
       '"", ""Category"" : ""' || regexp_replace(l.Code, '([""\])', '\\\1') ||  
       '""}}' as message 
       from PUNCHLISTITEM pl
           join TAGCHECK tc on tc.tagcheck_id = pl.tagcheck_id
           join projectschema ps on ps.projectschema = tc.projectschema
           left join Responsible r ON tc.Responsible_id = r.Responsible_Id
           left join TagFormularType tft ON tc.TagFormularType_Id = tft.TagFormularType_Id
           left join FormularType ft ON tft.FormularType_Id = ft.FormularType_Id
           left join Tag t on tft.Tag_Id = t.Tag_Id
           left join library reg on reg.library_id = tag.register_id
           left join Project p on p.Project_Id=t.Project_Id
           left join Library l on l.Library_Id = pl.Status_Id
       where tc.projectschema = '{schema}'";
    }
}