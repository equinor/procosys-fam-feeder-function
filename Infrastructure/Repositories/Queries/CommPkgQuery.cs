namespace Infrastructure.Repositories.Queries;

internal class CommPkgQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
        '{{""Plant"" : ""' || c.projectschema || 
         '"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') ||
         '"", ""ProjectName"" : ""' || p.name || 
         '"", ""CommPkgNo"" : ""' || c.COMMPKGNO ||
         '"", ""CommPkgId"" : ""' || c.COMMPKG_ID || 
         '"", ""Description"" : ""' || regexp_replace(c.DESCRIPTION, '([""\])', '\\\1') ||
         '"", ""DescriptionOfWork"" : ""' || regexp_replace(c.DESCRIPTIONOFWORK, '([""\])', '\\\1') ||
         '"", ""Remark"" : ""' || regexp_replace(c.REMARK, '([""\])', '\\\1') ||         
         '"", ""ResponsibleCode"" : ""' || regexp_replace(r.CODE, '([""\])', '\\\1')  ||                    
         '"", ""ResponsibleDescription"" : ""' || regexp_replace(r.DESCRIPTION, '([""\])', '\\\1')  ||                                 
         '"", ""AreaCode"" : ""' || CASE WHEN l.CODE IS NOT NULL THEN regexp_replace(l.CODE, '([""\])', '\\\1') ELSE '' END ||  
         '"", ""AreaDescription"" : ""' || CASE WHEN l.CODE IS NOT NULL THEN regexp_replace(l.DESCRIPTION, '([""\])', '\\\1') ELSE '' END ||  
         '"", ""Phase"" : ""' || phase.code ||
         '"", ""CommissioningIdentifier"" : ""' || identifier.code ||
         '"", ""IsVoided"" : ' || decode(e.isVoided,'Y', 'true', 'N', 'false') || 
         ', ""Demolition"" : ' || decode(c.DEMOLITION,'Y', 'true', 'N', 'false') ||
         ', ""CreatedAt"" : ""' || TO_CHAR(e.CREATEDAT, 'yyyy-mm-dd hh24:mi:ss') ||
         '"", ""Priority1"" : ""' || pri1.code  ||
         '"", ""Priority2"" : ""' || pri2.code  ||
         '"", ""Priority3"" : ""' || pri3.code  ||
         '"", ""LastUpdated"" : ""' || TO_CHAR(c.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:ss') ||                                    
         '""}}' as message
        from commpkg c
            join project p on p.project_id = c.project_id
            join projectschema ps on ps.projectschema = c.projectschema
            join RESPONSIBLE r on r.RESPONSIBLE_ID = c.RESPONSIBLE_ID
            join element e on e.element_id = c.commpkg_id
            left join library l on l.library_id = c.area_id
            left join library pri1 on pri1.library_id = c.COMMPRIORITY_ID
            left join library pri2 on pri2.library_id = c.COMMPRIORITY2_ID
            left join library pri3 on pri3.library_id = c.COMMPRIORITY3_ID
            left join library phase on phase.library_id = c.COMMPHASE_ID
            left join library identifier on identifier.library_id = c.IDENTIFIER_ID
        where c.projectschema ='{schema}'";
    }
}