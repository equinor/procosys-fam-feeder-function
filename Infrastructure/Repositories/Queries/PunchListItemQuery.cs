namespace Infrastructure.Repositories.Queries;

internal class PunchListItemQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
       '{{""Plant"" : ""' || pl.projectschema || 
       '"", ""PlantName"" : ""' || regexp_replace(ps.title, '([""\])', '\\\1') ||                          
       '"", ""ProjectName"" : ""' || regexp_replace(p.Title, '([""\])', '\\\1') || 
       '"", ""LastUpdated"" : ""' || TO_CHAR(pl.LAST_UPDATED, 'YYYY-MM-DD hh:mm:ss') ||                                                 
       '"", ""PunchItemNo"" : ""' || pl.PunchListItem_Id || 
       '"", ""Description"" : ""' || regexp_replace(pl.Description, '([""\])', '\\\1') || 
       '"", ""TagNo"" : ""' || regexp_replace(t.TagNo, '([""\])', '\\\1') || 
       '"", ""ResponsibleCode"" : ""' || regexp_replace(r.Code, '([""\])', '\\\1')  ||                    
       '"", ""ResponsibleDescription"" : ""' || regexp_replace(r.Description, '([""\])', '\\\1')  ||                                 
       '"", ""FormType"" : ""' || regexp_replace(ft.FormularType, '([""\])', '\\\1') ||  
       '"", ""Category"" : ""' || regexp_replace(l.Code, '([""\])', '\\\1') ||  
       '""}}' as message 
       from PUNCHLISTITEM pl
           join TAGCHECK tc on tc.tagcheck_id = pl.tagcheck_id
           join projectschema ps on ps.projectschema = tc.projectschema
           LEFT JOIN Responsible r ON tc.Responsible_id = r.Responsible_Id
           LEFT JOIN TagFormularType tft ON tc.TagFormularType_Id = tft.TagFormularType_Id
           LEFT JOIN FormularType ft ON tft.FormularType_Id = ft.FormularType_Id
           LEFT JOIN Tag t on tft.Tag_Id = t.Tag_Id
           LEFT JOIN Project p on p.Project_Id=t.Project_Id
           LEFT JOIN Library l on l.Library_Id = pl.Status_Id
       where tc.projectschema = '{schema}'";
    }
}