namespace Infrastructure.Repositories.Queries;

internal class TagQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
              '{{'||
              '""TagNo"" : ""' || regexp_replace(t.tagno, '([""\])', '\\\1') || '"",' ||
              '""Description"" : ""' || regexp_replace(t.description, '([""\])', '\\\1') || '"",'||
              '""ProjectName"" : ""' || p.name || '"",' ||
              '""McPkgNo"" : ""' || mcpkg.mcpkgno || '"",' ||
              '""CommPkgNo"" : ""' || commpkg.commpkgno || '"",' ||
              '""TagId"" : ""' || t.tag_id || '"",' ||
              '""AreaCode"" : ""' || area.code || '"",' ||
              '""AreaDescription"" : ""' || regexp_replace(area.description, '([""\])', '\\\1') || '"",' ||
              '""DisciplineCode"" : ""' || discipline.code || '"",' ||
              '""DisciplineDescription"" : ""' || regexp_replace(discipline.description, '([""\])', '\\\1') || '"",' ||
              '""RegisterCode"" : ""' || register.code || '"",' ||
              '""Status"" : ""' || status.code || '"",' ||
              '""System"" : ""' || system.code || '"",' ||
              '""CallOffNo"" : ""' || calloff.calloffno || '"",' ||
              '""PurchaseOrderNo"" : ""' || purchaseorder.packageno || '"",' ||
              '""TagFunctionCode"" : ""' || tagfunction.tagfunctioncode || '"",' ||
              '""IsVoided"" : ' || decode(e.IsVoided,'Y', 'true', 'N', 'false') || ',' ||
              '""Plant"" : ""' || t.projectschema || '"",' ||
              '""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') || '"",' ||    
              '""LastUpdated"" : ""' || TO_CHAR(t.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:sss') || '"",' ||
              '""TagDetails"" : {{' || 
              (SELECT listagg('""'|| colName ||'"":""'|| regexp_replace(val, '([""\])', '\\\1') ||'""', ',')
                WITHIN group (order by colName) as tagdetails  from (
                SELECT 
                       f.columnname as colName,
                        COALESCE(regexp_replace(val.valuestring, '([""\])', '\\\1'),
                                 TO_CHAR(VAL.VALUEDATE, 'yyyy-mm-dd hh24:mi:sss'),
                                 TO_CHAR(VAL.VALUENUMBER),
                                 t2.TAGNO, 
                                 LIBVAL.CODE
                                 ) as val
                FROM DEFINEELEMENTFIELD DEF
                    LEFT JOIN FIELD F ON DEF.FIELD_ID = F.FIELD_ID
                    LEFT JOIN LIBRARY UNIT ON UNIT.LIBRARY_ID = F.UNIT_ID
                    JOIN ELEMENTFIELD VAL
                        ON (VAL.FIELD_ID = DEF.FIELD_ID AND VAL.ELEMENT_ID = t.tag_id)
                    JOIN TAG t1 on t1.TAG_ID = VAL.ELEMENT_ID  
                    LEFT JOIN LIBRARY LIBVAL ON (LIBVAL.LIBRARY_ID = VAL.LIBRARY_ID)
                    LEFT JOIN LIBRARY REG ON REG.LIBRARY_ID = DEF.REGISTER_ID
                    LEFT JOIN TAG t2 ON t2.TAG_ID = VAL.TAG_ID
                WHERE DEF.ELEMENTTYPE = 'TAG'
                AND (DEF.REGISTER_ID IS NULL OR DEF.REGISTER_ID = t.register_id)
                AND NOT (DEF.ISVOIDED = 'Y')
                AND F.COLUMNTYPE in ('NUMBER','DATE','STRING', 'LIBRARY','TAG')
                AND f.projectschema ='{schema}'))
                || '}}' ||
                '}}' as message
                from tag t
                    join element e on e.element_id = t.tag_id
                    join projectschema ps on ps.projectschema = t.projectschema
                    left join mcpkg on mcpkg.mcpkg_id=t.mcpkg_id
                    left join commpkg on commpkg.commpkg_id=mcpkg.commpkg_id
                    left join project p on p.project_id=t.project_id
                    left join tagfunction tf on tf.tagfunction_id = t.tagfunction_id
                    left join library area on area.library_id=t.area_id
                    left join library discipline on discipline.library_id=t.discipline_id
                    left join library register on register.library_id=t.register_id
                    left join library status on status.library_id=t.status_id
                    left outer join library system on system.library_id=t.system_id
                    left join calloff  on calloff.calloff_id=t.calloff_id
                    left join purchaseorder on purchaseorder.package_id=calloff.package_id
                    left join tagfunction on tagfunction.tagfunction_id = t.tagfunction_id
                where t.projectschema = '{schema}'";
    }
}