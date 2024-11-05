using Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using Core.Interfaces;
using Core.Models;
using Dapper;
using Infrastructure.Handlers;
using Infrastructure.Repositories.SearchQueries;

namespace Infrastructure.Repositories;

public class WorkOrderCutoffRepository : IWorkOrderCutoffRepository
{
    private readonly AppDbContext _context;

    public WorkOrderCutoffRepository(AppDbContext context) => _context = context;

    public async Task<List<string>> GetWoCutoffs(string weekNumber, string plant)
    {
        var dbConnection = _context.Database.GetDbConnection();
        var query = WorkOrderCutoffQuery.GetQuery(null, null, plant, weekNumber);
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