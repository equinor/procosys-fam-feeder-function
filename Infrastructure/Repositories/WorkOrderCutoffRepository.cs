using Infrastructure.Data;
using Equinor.ProCoSys.PcsServiceBus.Queries;

using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using Core.Interfaces;
using Core.Models;
using Dapper;
using Infrastructure.Handlers;

namespace Infrastructure.Repositories;

public class WorkOrderCutoffRepository : IWorkOrderCutoffRepository
{
    private readonly AppDbContext _context;

    public WorkOrderCutoffRepository(AppDbContext context) => _context = context;

    public async Task<List<string>> GetWoCutoffs(string month, string plant)
    {
        var dbConnection = _context.Database.GetDbConnection();
        await using var command = dbConnection.CreateCommand();
        var query = WorkOrderCutoffQuery.GetQuery(null, null, plant, month);
        command.CommandText = query.queryString;
        var connectionWasClosed = dbConnection.State != ConnectionState.Open;
        if (connectionWasClosed)
        {
            await _context.Database.OpenConnectionAsync();
        }
        try
        {
            var events = dbConnection.Query<WorkOrderCutoff>(query.queryString, query.parameters).ToList();
            if (events.Count == 0)
            {
                return new List<string>();
            }
            var serializedWoCutoffs = events.Select(e => JsonSerializer.Serialize(e,DefaultSerializerHelper.SerializerOptions)).ToList();
            return serializedWoCutoffs;
        }
        finally
        {
            //If we open it, we have to close it.
            if (connectionWasClosed)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
   
    }
}