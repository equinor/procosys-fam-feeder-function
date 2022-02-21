namespace Infrastructure.Repositories.Queries;

internal class WorkOrderCutoffQuery
{
    internal static string GetQuery(string schema, string month)
    {
        return @$"select
        '{{""Plant"" : ""' || wc.projectschema || 
        '"", ""PlantName"" : ""' || regexp_replace(ps.TITLE, '([""\])', '\\\1') ||          
        '"", ""ProjectName"" : ""' || p.NAME || 
        '"", ""WoNo"" : ""' || wo.WONO ||
        '"", ""JobStatusCode"" : ""' || regexp_replace(jsc.CODE, '([""\])', '\\\1') ||
        '"", ""MaterialStatusCode"" : ""' || regexp_replace(msc.CODE, '([""\])', '\\\1') ||
        '"", ""DisciplineCode"" : ""' || regexp_replace(dc.CODE, '([""\])', '\\\1') ||
        '"", ""CategoryCode"" : ""' || regexp_replace(cat.CODE, '([""\])', '\\\1') ||  
        '"", ""MilestoneCode"" : ""' || regexp_replace(milestone.CODE, '([""\])', '\\\1') ||
        '"", ""SubMilestoneCode"" : ""' || regexp_replace(submilestone.CODE, '([""\])', '\\\1') ||
        '"", ""HoldByCode"" : ""' ||  regexp_replace(hbc.CODE, '([""\])', '\\\1') ||
        '"", ""PlanActivityCode"" : ""' ||  regexp_replace(pa.CODE, '([""\])', '\\\1') ||
        '"", ""ResponsibleCode"" : ""' || regexp_replace(r.CODE, '([""\])', '\\\1')  ||                                            
        '"", ""LastUpdated"" : ""' || TO_CHAR(wc.LAST_UPDATED, 'YYYY-MM-DD hh:mm:ss') ||
        '"", ""CutoffWeek"" : ""' || wc.CUTOFFWEEK ||    
        '"", ""CutoffDate"" : ""' || TO_CHAR(wc.CUTOFFDATE, 'YYYY-MM-DD hh:mm:ss') ||   
        '"", ""PlannedStartAtDate"" : ""' || TO_CHAR(wc.WOPLANNEDSTARTUPDATE, 'YYYY-MM-DD hh:mm:ss')  ||
        '"", ""PlannedFinishAtDate"" : ""' || TO_CHAR(wc.WOPLANNEDCOMPLETIONDATE, 'YYYY-MM-DD hh:mm:ss')  ||
        '"", ""ManhoursExpended"" : ""' || wc.EXPENDED_MHRS || 
        '"", ""ManhoursEarned"" : ""' || wc.EARNED_MHRS  ||
        '"", ""ManhoursEstimated"" : ""' || wc.ESTIMATED_MHRS  ||
        '"", ""ManhoursExpendedLastWeek"" : ""' ||wc.EXPENDED_LW  ||  
        '"", ""ManhoursEarnedLastWeek"" : ""' || wc.EARNED_LW  ||  
        '""}}' as message
        from wo_cutoff wc
            join wo wo on wo.wo_id = wc.wo_id
            join projectschema ps ON ps.projectschema = wc.projectschema
            join project p ON p.project_id = wc.project_id and p.isvoided = 'N' and p.isclosed = 'N'
            left join library milestone ON milestone.library_id = wc.womilestone_id
            left join library submilestone on submilestone.library_id = wc.wosubmilestone_id
            left join library cat ON cat.library_id = wc.category_id
            left join library msc ON msc.library_id = wc.materialstatus_id
            left join library dc ON dc.library_id = wc.discipline_id
            left join library hbc ON hbc.library_id = wc.holdby_id
            left join library pa ON pa.library_id = wc.planactivity_id
            left join library r ON r.library_id = wc.WORESPONSIBLE_id
            left join library jsc ON jsc.library_id = wc.jobstatus_id
            left join library area ON area.library_id = wc.area_id
        where wc.projectschema = '{schema}' and cutoffdate like '%.{month}.%'";
    }
}