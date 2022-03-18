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

    public async Task<List<FamEvent>> GetMcPackages(string plant)
    {
        return await ExecuteQuery(McPkgQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetCommPackages(string plant)
    {
        return await ExecuteQuery(CommPkgQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetPunchItems(string plant)
    {
        return await ExecuteQuery(PunchListItemQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetWorkOrders(string plant)
    {
        return await ExecuteQuery(WorkOrderQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetCheckLists(string plant)
    {
        return await ExecuteQuery(ChecklistQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetTags(string plant)
    {
        return await ExecuteQuery(TagQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetMilestones(string plant)
    {
        return await ExecuteQuery(MilestonesQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetProjects(string plant)
    {
        return await ExecuteQuery(ProjectQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetSwcr(string plant)
    {
        return await ExecuteQuery(SwcrQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetSwcrSignature(string plant)
    {
        return await ExecuteQuery(SwcrSignatureQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetWoChecklists(string plant)
    {
        return await ExecuteQuery(WorkOrderChecklistsQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetQuery(string plant)
    {
        return await ExecuteQuery(Query.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetQuerySignature(string plant)
    {
        return await ExecuteQuery(QuerySignature.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetPipingRevision(string plant)
    {
        return await ExecuteQuery(PipingRevisionQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetWoMilestones(string plant)
    {
        return await ExecuteQuery(WorkOrderMilestoneQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetWoMaterials(string plant)
    {
        return await ExecuteQuery(WorkOrderMaterialQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetStock(string plant)
    {
        return await ExecuteQuery(StockQuery.GetQuery(plant));
    }

    public async Task<List<FamEvent>> GetWoCutoffs(string month, string plant, string? connectionString)
    {
        return await _workOrderCutoffRepository.GetWoCutoffs(month, plant, connectionString);
    }

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