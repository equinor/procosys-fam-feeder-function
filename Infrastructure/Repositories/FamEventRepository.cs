using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;
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

    public async Task<List<FamEvent>> GetMcPackages(string plant) => await ExecuteQuery(McPkgQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetCommPackages(string plant) => await ExecuteQuery(CommPkgQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetCommPkgOperations(string plant) => await ExecuteQuery(CommPkgOperationQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetPunchItems(string plant) => await ExecuteQuery(PunchListItemQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetWorkOrders(string plant) => await ExecuteQuery(WorkOrderQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetCheckLists(string plant) => await ExecuteQuery(ChecklistQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetTags(string plant) => await ExecuteQuery(TagQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetMilestones(string plant) => await ExecuteQuery(MilestonesQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetProjects(string plant) => await ExecuteQuery(ProjectQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetSwcr(string plant) => await ExecuteQuery(SwcrQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetSwcrSignature(string plant) => await ExecuteQuery(SwcrSignatureQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetWoChecklists(string plant) => await ExecuteQuery(WorkOrderChecklistsQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetQuery(string plant) => await ExecuteQuery(Query.GetQuery(plant));
    public async Task<List<FamEvent>> GetQuerySignature(string plant) => await ExecuteQuery(QuerySignature.GetQuery(plant));
    public async Task<List<FamEvent>> GetPipingRevision(string plant) => await ExecuteQuery(PipingRevisionQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetPipingSpool(string plant) => await ExecuteQuery(PipingSpoolQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetWoMilestones(string plant) => await ExecuteQuery(WorkOrderMilestoneQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetWoMaterials(string plant) => await ExecuteQuery(WorkOrderMaterialQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetStock(string plant) => await ExecuteQuery(StockQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetResponsible(string plant) => await ExecuteQuery(ResponsibleQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetLibrary(string plant) => await ExecuteQuery(LibraryQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetWoCutoffs(string month, string plant, string? connectionString) => await _workOrderCutoffRepository.GetWoCutoffs(month, plant, connectionString);
    public async Task<List<FamEvent>> GetDocument(string plant) => await ExecuteQuery(DocumentQuery.GetQuery(plant));
    public async Task<List<FamEvent>> GetLoopContent(string plant) => await ExecuteQuery(LoopContentQuery.GetQuery(plant));

    internal async Task<List<FamEvent>> ExecuteQuery(string query)
    {
        await using var context = _context;
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        await context.Database.OpenConnectionAsync();
        await using var result = await command.ExecuteReaderAsync();
        var entities = new List<FamEvent>();

        while (await result.ReadAsync())
        {
            if (result.HasRows)
            {
                entities.Add(new FamEvent { Message = (string)result[0] });
            }
        }
        return entities;
    }
}