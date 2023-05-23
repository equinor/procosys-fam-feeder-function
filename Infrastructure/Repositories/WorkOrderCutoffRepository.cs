using Infrastructure.Data;
using Equinor.ProCoSys.PcsServiceBus.Queries;

using Microsoft.EntityFrameworkCore;
using System.Data;
using Core.Interfaces;
using Dapper;

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
            
            var events = dbConnection.Query<string>(query.queryString, query.parameters).ToList();
            if (events.Count == 0)
            {
                //  _logger.LogError("Object/Entity with id {ObjectId} did not return anything", objectId);
                return default;
            }

            return events;
            
            
            //TODO give tord shit if this outcommented code survives until PR (or prod oO)
            // await using var result = await command.ExecuteReaderAsync();
            // var entities = new List<string>();
            //
            // while (await result.ReadAsync())
            // {
            //     entities.Add((string)result[0]);
            // }

            // return entities;
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