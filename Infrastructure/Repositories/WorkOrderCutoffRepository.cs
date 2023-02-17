using Infrastructure.Data;
using Equinor.ProCoSys.PcsServiceBus.Queries;

using Microsoft.EntityFrameworkCore;
using System.Data;
using Core.Interfaces;

namespace Infrastructure.Repositories;

public class WorkOrderCutoffRepository : IWorkOrderCutoffRepository
{
    private readonly AppDbContext _context;

    public WorkOrderCutoffRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetWoCutoffs(string month, string plant)
    {
        var dbConnection = _context.Database.GetDbConnection();
        await using var command = dbConnection.CreateCommand();
        command.CommandText = WorkOrderCutoffQuery.GetQuery(null, null, plant, month);
        var connectionWasClosed = dbConnection.State != ConnectionState.Open;
        if (connectionWasClosed)
        {
            await _context.Database.OpenConnectionAsync();
        }
        try
        {
            await using var result = await command.ExecuteReaderAsync();
            var entities = new List<string>();

            while (await result.ReadAsync())
            {
                entities.Add((string)result[0]);
            }

            return entities;
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