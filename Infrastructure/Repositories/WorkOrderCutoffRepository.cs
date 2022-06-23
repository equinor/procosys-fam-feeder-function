using Infrastructure.Data;
using Equinor.ProCoSys.PcsServiceBus.Queries;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class WorkOrderCutoffRepository
{
    public async Task<List<string>> GetWoCutoffs(string month, string plant, string? connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseOracle(connectionString!, b => b.MaxBatchSize(200));
        await using var context = new AppDbContext(options.Options);
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = WorkOrderCutoffQuery.GetQuery(plant, month);
        await context.Database.OpenConnectionAsync();
        await using var result = await command.ExecuteReaderAsync();
        var entities = new List<string>();

        while (await result.ReadAsync())
        {
            entities.Add( (string)result[0]);
        }
        return entities;
    }
}