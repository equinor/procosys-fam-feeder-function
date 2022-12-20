using Core.Interfaces;
using Core.Models.DbStatus;
using Infrastructure.Data;
using Infrastructure.Repositories.DbStatusQueries;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DbStatusRepository : IDbStatusRepository
{
    private readonly AppDbContext _context;

    public DbStatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MetricDto>> GetMetrics() => await ExecuteQuery(MetricQuery.GetQuery());

    internal async Task<List<MetricDto>> ExecuteQuery(string query)
    {
        await using var context = _context;
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        await context.Database.OpenConnectionAsync();
        await using var result = await command.ExecuteReaderAsync();
        
        var metrics = new List<MetricDto>();

        while (await result.ReadAsync())
        {
            metrics.Add(new MetricDto()
            {
                UseName = result.IsDBNull(0) ? "" : result.GetString(0),
                Program = result.IsDBNull(1) ? "" : result.GetString(1),
                Sid = result.GetInt32(2),
                Serial = result.GetInt32(3),
                Name = result.IsDBNull(4) ? "" : result.GetString(4),
                Value = result.GetInt32(5),
            }); 
        }
        return metrics;
    }
}