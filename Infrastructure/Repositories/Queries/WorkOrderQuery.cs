
namespace Infrastructure.Repositories.Queries;

internal class WorkOrderQuery
{

    internal static string GetQuery(string schema)
    {
        return @$"select
         '{{""Plant"" : ""' || w.projectschema || 
         '"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') ||          
         '"", ""ProjectName"" : ""' || p.NAME || 
         '"", ""WoNo"" : ""' || w.WONO ||
         '"", ""WoId"" : ""' || w.WO_ID ||
         '"", ""CommPkgNo"" : ""' || c.COMMPKGNO || 
         '"", ""Title"" : ""' || regexp_replace(w.DESCRIPTIONSHORT, '([""\])', '\\\1') || 
         '"", ""Description"" : ""' || regexp_replace(w.DESCRIPTIONLONG, '([""\])', '\\\1') ||
         '"", ""MilestoneCode"" : ""' || regexp_replace(milestone.CODE, '([""\])', '\\\1') ||
         '"", ""SubMilestoneCode"" : ""' || regexp_replace(submilestone.CODE, '([""\])', '\\\1') ||
         '"", ""MilestoneDescription"" : ""' || regexp_replace(milestone.description, '([""\])', '\\\1') ||   
         '"", ""CategoryCode"" : ""' || regexp_replace(cat.CODE, '([""\])', '\\\1') ||  
         '"", ""MaterialStatusCode"" : ""' || regexp_replace(msc.CODE, '([""\])', '\\\1') ||
         '"", ""HoldByCode"" : ""' ||  regexp_replace(hbc.CODE, '([""\])', '\\\1') ||
         '"", ""DisciplineCode"" : ""' ||  regexp_replace(dis.CODE, '([""\])', '\\\1') ||         
         '"", ""DisciplineDescription"" : ""' ||  regexp_replace(dis.description, '([""\])', '\\\1') ||         
         '"", ""ResponsibleCode"" : ""' || regexp_replace(r.CODE, '([""\])', '\\\1')  ||                    
         '"", ""ResponsibleDescription"" : ""' || regexp_replace(r.description, '([""\])', '\\\1')  ||                                 
         '"", ""AreaCode"" : ""' || regexp_replace(area.CODE, '([""\])', '\\\1') ||
         '"", ""AreaDescription"" : ""' || regexp_replace(area.description, '([""\])', '\\\1') ||  
         '"", ""JobStatusCode"" : ""' || regexp_replace(jsc.CODE, '([""\])', '\\\1') ||
         '"", ""TypeOfWorkCode"" : ""' || regexp_replace(tow.CODE, '([""\])', '\\\1') || 
         '"", ""OnShoreOffShoreCode"" : ""' || regexp_replace(osos.CODE, '([""\])', '\\\1') || 
         '"", ""WoTypeCode"" : ""' || regexp_replace(woc.CODE, '([""\])', '\\\1') ||   
         '"", ""ProjectProgress"" : ""' || regexp_replace(w.PROJECTPROGRESS, '([""\])', '\\\1') ||
         '"", ""ExpendedManHours"" : ""' || regexp_replace(NVL(w.TOTALEXPENDEDMANHOURS, w.expended_mhrs), '([""\])', '\\\1') ||
         '"", ""EstimatedHours"" : ""' || 
                  ( Select ROUND (
                        SUM (quantity
                        * multiplicator
                        * disciplinefactor
                        * normvalue), 1)
                    FROM wo_estimate we
                    WHERE we.wo_id = w.wo_id  )
                  ||
         '"", ""RemainingHours"" : ""' || 
                 ( SELECT  ROUND (
                     SUM (quantity
                        * multiplicator
                         * disciplinefactor
                         * normvalue
                         ) * (1 - w.PROJECTPROGRESS / 100), 1)
                FROM wo_estimate we
                WHERE we.wo_id = w.wo_id)
                 ||
         '"", ""PlannedStartAtDate"" : ""' || TO_CHAR(w.WOPLANNEDSTARTUPDATE, 'YYYY-MM-DD hh:mm:ss') ||
         '"", ""ActualStartAtDate"" : ""' || TO_CHAR(w.WOACTUALSTARTUPDATE, 'YYYY-MM-DD hh:mm:ss') ||
         '"", ""PlannedFinishedAtDate"" : ""' || TO_CHAR(w.WOPLANNEDCOMPLETIONDATE, 'YYYY-MM-DD hh:mm:ss') ||
         '"", ""ActualFinishedAtDate"" : ""' ||  TO_CHAR(w.WOACTUALCOMPLETIONDATE, 'YYYY-MM-DD hh:mm:ss') ||
         '"", ""CreatedAt"" : ""' || TO_CHAR(e.CREATEDAT, 'YYYY-MM-DD hh:mm:ss') ||
         '"", ""IsVoided"" : ""' || decode(e.isVoided,'Y', 'true', 'N', 'false') ||
         '"", ""LastUpdated"" : ""' || TO_CHAR(w.LAST_UPDATED, 'YYYY-MM-DD hh:mm:ss') ||        
         '""}}' as message
         from WO w
            join projectschema ps on ps.projectschema = w.projectschema
            join project p on p.project_id = w.project_id
            join element e on E.ELEMENT_ID = w.wo_ID and e.isvoided = 'N'
            left join commpkg c on c.commpkg_id = w.commpkg_id
            left join library milestone on milestone.library_id = w.womilestone_id
            left join library submilestone on submilestone.library_id = w.wosubmilestone_id
            left join library cat on cat.library_id = w.category_id
            left join library msc on msc.library_id = w.materialstatus_id
            left join library hbc on hbc.library_id = w.holdby_id
            left join library dis on dis.library_id = w.discipline_id
            left join library r on r.library_id = w.WORESPONSIBLE_id
            left join library area on area.library_id = w.area_id
            left join library jsc on jsc.library_id = w.jobstatus_id
            left join library tow on tow.library_id = w.typeofwork_id
            left join library osos on osos.library_id = w.onshoreoffshore_id
            left join library woc on woc.library_id = w.wo_id
         where w.projectschema = '{schema}'";
    }
}