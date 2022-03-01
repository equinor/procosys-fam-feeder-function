using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace Infrastructure;

public static class ServiceCollectionSetup
{
    private const int MaxOpenCursors = 200;

    public static readonly LoggerFactory LoggerFactory =
        new(new[]
        {
            new DebugLoggerProvider()
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