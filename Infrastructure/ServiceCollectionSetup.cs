using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class ServiceCollectionSetup
{
    /**
     * Maximum open cursors in the Pcs database is configured to 300 as per 05.03.2020.
     * When doing batch updates/inserts, oracle opens a cursor per update/insert to keep track of
     * the amount of entities updated. The default seems to be 200, but we're setting it explicitly anyway
     * in case the default changes in the future. This is to avoid ORA-01000: maximum open cursors exceeded.
     */
    private const int MaxOpenCursors = 200;

    public static readonly LoggerFactory LoggerFactory =
        new(new[]
        {
            new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
        });

    public static IServiceCollection AddDbContext(this IServiceCollection services, string connectionString)
    {
        return services.AddDbContext<AppDbContext>(options =>
        {
            options.UseOracle(connectionString, b => b.MaxBatchSize(MaxOpenCursors));
            options.UseLoggerFactory(LoggerFactory);
            options.EnableSensitiveDataLogging();
        });
    }
}