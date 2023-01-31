using Core.Interfaces;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus.Queries;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FamEventRepository : IFamEventRepository
{
    private readonly AppDbContext _context;
    private readonly WorkOrderCutoffRepository _workOrderCutoffRepository;

    public FamEventRepository(AppDbContext context)
    {
        _context = context;
        _workOrderCutoffRepository = new WorkOrderCutoffRepository();
    }
    public async Task<List<string>> GetSwcrAttachments(string plant) => await ExecuteQuery(SwcrAttachmentQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcrOtherReferences(string plant) => await ExecuteQuery(SwcrTypeQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcrType(string plant) => await ExecuteQuery(SwcrOtherReferencesQuery.GetQuery(null, plant));
    public async Task<List<string>> GetActions(string plant) => await ExecuteQuery(ActionQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCommPkgTasks(string plant) => await ExecuteQuery(CommPkgTaskQuery.GetQuery(null, null, plant));
    public async Task<List<string>> GetTasks(string plant) => await ExecuteQuery(TaskQuery.GetQuery(null, plant));
    public async Task<List<string>> GetMcPackages(string plant) => await ExecuteQuery(McPkgQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPackages(string plant) => await ExecuteQuery(CommPkgQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPkgOperations(string plant) => await ExecuteQuery(CommPkgOperationQuery.GetQuery(null,plant));
    public async Task<List<string>> GetPunchItems(string plant) => await ExecuteQuery(PunchListItemQuery.GetQuery(null,plant));
    public async Task<List<string>> GetWorkOrders(string plant) => await ExecuteQuery(WorkOrderQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCheckLists(string plant) => await ExecuteQuery(ChecklistQuery.GetQuery(null,plant));
    public async Task<List<string>> GetTags(string plant) => await ExecuteQuery(TagQuery.GetQuery(null,plant));
    public async Task<List<string>> GetMilestones(string plant) => await ExecuteQuery(MilestonesQuery.GetQuery(null,null,plant));
    public async Task<List<string>> GetProjects(string plant) => await ExecuteQuery(ProjectQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcr(string plant) => await ExecuteQuery(SwcrQuery.GetQuery(null,plant));
    public async Task<List<string>> GetSwcrSignature(string plant) => await ExecuteQuery(SwcrSignatureQuery.GetQuery(null, plant));
    public async Task<List<string>> GetWoChecklists(string plant) => await ExecuteQuery(WorkOrderChecklistsQuery.GetQuery(null,null, plant));
    public async Task<List<string>> GetQuery(string plant) => await ExecuteQuery(QueryQuery.GetQuery(null,plant));
    public async Task<List<string>> GetQuerySignature(string plant) => await ExecuteQuery(QuerySignatureQuery.GetQuery(null, plant));
    public async Task<List<string>> GetPipingRevision(string plant) => await ExecuteQuery(PipingRevisionQuery.GetQuery(null,plant));
    public async Task<List<string>> GetPipingSpool(string plant) => await ExecuteQuery(PipingSpoolQuery.GetQuery(null,plant));
    public async Task<List<string>> GetWoMilestones(string plant) => await ExecuteQuery(WorkOrderMilestoneQuery.GetQuery(null,null,plant));
    public async Task<List<string>> GetWoMaterials(string plant) => await ExecuteQuery(WorkOrderMaterialQuery.GetQuery(null,plant));
    public async Task<List<string>> GetStock(string plant) => await ExecuteQuery(StockQuery.GetQuery(null,plant));
    public async Task<List<string>> GetResponsible(string plant) => await ExecuteQuery(ResponsibleQuery.GetQuery(null, plant));
    public async Task<List<string>> GetLibrary(string plant) => await ExecuteQuery(LibraryQuery.GetQuery(null, plant));
    public async Task<List<string>> GetWoCutoffs(string month, string plant, string? connectionString) => await _workOrderCutoffRepository.GetWoCutoffs(month, plant, connectionString);
    public async Task<List<string>> GetDocument(string plant) => await ExecuteQuery(DocumentQuery.GetQuery(null,plant));
    public async Task<List<string>> GetLoopContent(string plant) => await ExecuteQuery(LoopContentQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCallOff(string plant) => await ExecuteQuery(CallOffQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPkgQuery(string plant) => await ExecuteQuery(QueryCommPkgQuery.GetQuery(null, null, plant));
    public async Task<List<string>> GetWoCutoffsByWeekAndPlant(string cutoffWeek, string plant) => await ExecuteQuery(WorkOrderCutoffQuery.GetQuery(null, cutoffWeek, plant,null));
    public async Task<List<string>> GetHeatTrace(string plant) => await ExecuteQuery(HeatTraceQuery.GetQuery(null, plant));
  

    internal async Task<List<string>> ExecuteQuery(string query)
    {
        await using var context = _context;
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        await context.Database.OpenConnectionAsync();
        await using var result = await command.ExecuteReaderAsync();
        var entities = new List<string>();

        while (await result.ReadAsync())
        {
            if (result.HasRows)
            {
                var s = (string)result[0];
                
                entities.Add(s.Replace("\"WoNo\" : \"���\"", "\"WoNo\" : \"ÆØÅ\"") );
            }
        }
        return entities;
    }    
}