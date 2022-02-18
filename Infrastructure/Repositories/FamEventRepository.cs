using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories;

public class FamEventRepository : IFamEventRepository
{
    private readonly AppDbContext _context;

    private readonly WorkOrderCutoffRepository _workOrderCutoffRepository;

    public FamEventRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _workOrderCutoffRepository = new WorkOrderCutoffRepository();
    }

    public async Task<List<FamEvent>> GetMcPackages(string plant) => await ExecuteQuery(McPkgQuery.GetQuery(plant));

    public async Task<List<FamEvent>> GetCommPackages(string plant) => await ExecuteQuery(CommPkgQuery.GetQuery(plant));

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

    public async Task<List<FamEvent>> GetWoCutoffs(string month, string plant, string connectionString)
        => await _workOrderCutoffRepository.GetWoCutoffs(month,plant, connectionString);

    internal async Task<List<FamEvent>> ExecuteQuery(string query)
    {
        await using var context = _context;
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        await context.Database.OpenConnectionAsync();
        await using var result = await command.ExecuteReaderAsync();
        var entities = new List<FamEvent>();

        while (await result.ReadAsync()) entities.Add(new FamEvent { Message = (string)result[0] });
        return entities;
    }
}