using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class WorkOrderCutoffRepository
{
    public async Task<List<FamEvent>> GetWoCutoffs(string month, string connectionstring)
    {

        var query = @$"SELECT
    '{"{"}""Plant"" : ""' || wc.projectschema || 
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
    '""{"}"}' as message
  FROM wo_cutoff wc
    Join wo wo on wo.wo_id = wc.wo_id
    JOIN projectschema ps ON ps.projectschema = wc.projectschema
    JOIN project p ON p.project_id = wc.project_id and p.isvoided = 'N' and p.isclosed = 'N'
    LEFT JOIN library milestone ON milestone.library_id = wc.womilestone_id
    LEFT JOIN library submilestone on submilestone.library_id = wc.wosubmilestone_id
    LEFT JOIN library cat ON cat.library_id = wc.category_id
    LEFT JOIN library msc ON msc.library_id = wc.materialstatus_id
    LEFT JOIN library dc ON dc.library_id = wc.discipline_id
    LEFT JOIN library hbc ON hbc.library_id = wc.holdby_id
    LEFT JOIN library pa ON pa.library_id = wc.planactivity_id
    LEFT JOIN library r ON r.library_id = wc.WORESPONSIBLE_id
    LEFT JOIN library jsc ON jsc.library_id = wc.jobstatus_id
    LEFT JOIN library area ON area.library_id = wc.area_id
  WHERE wc.projectschema = 'PCS$JOHAN_CASTBERG' and cutoffdate like '%.{month}.%'";


        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseOracle(connectionstring, b => b.MaxBatchSize(200));


        await using var context = new AppDbContext(options.Options);
         
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;

        await context.Database.OpenConnectionAsync();

        await using var result = await command.ExecuteReaderAsync();
        var entities = new List<FamEvent>();

        while (await result.ReadAsync()) entities.Add(new FamEvent { Message = (string)result[0] });

        return entities;
    }
}