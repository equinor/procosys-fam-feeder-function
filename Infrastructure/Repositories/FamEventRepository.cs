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

    public async Task<List<FamEvent>> GetMcPackages() => await ExecuteQuery(McPkgQuery.Query);

    public async Task<List<FamEvent>> GetCommPackages() => await ExecuteQuery(CommPkgQuery.Query);

    public async Task<List<FamEvent>> GetPunchItems() => await ExecuteQuery(PunchListItemQuery.Query);

    public async Task<List<FamEvent>> GetWorkOrders() => await ExecuteQuery(WorkOrderQuery.Query);

    public async Task<List<FamEvent>> GetCheckLists() => await ExecuteQuery(ChecklistQuery.Query);

    public async Task<List<FamEvent>> GetTags() => await ExecuteQuery(TagQuery.Query);

    public async Task<List<FamEvent>> GetMilestones() => await ExecuteQuery(MilestonesQuery.Query);

    public async Task<List<FamEvent>> GetProjects() => await ExecuteQuery(ProjectQuery.Query);

    public async Task<List<FamEvent>> GetSwcr() => await ExecuteQuery(SwcrQuery.Query);

    public async Task<List<FamEvent>> GetSwcrSignature() => await ExecuteQuery(SwcrSignatureQuery.Query);

    public async Task<List<FamEvent>> GetWoChecklists() => await ExecuteQuery(WorkOrderChecklistsQuery.Query);

    public async Task<List<FamEvent>> GetQuery() => await ExecuteQuery(Query.SqlQuery);

    public async Task<List<FamEvent>> GetQuerySignature() => await ExecuteQuery(QuerySignature.SqlQuery);

    public async Task<List<FamEvent>> GetWoCutoffs(string month, string connectionString)
        => await _workOrderCutoffRepository.GetWoCutoffs(month, connectionString);

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