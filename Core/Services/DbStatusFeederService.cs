using Core.Interfaces;
using Core.Models.DbStatus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Core.Services;

public class DbStatusFeederService : IDbStatusFeederService
{
    private readonly FamFeederOptions _famFeederOptions;
    private readonly IDbStatusRepository _dbStatusRepo;

    public DbStatusFeederService(
        IDbStatusRepository dbStatusRepo,
        IOptions<FamFeederOptions> famFeederOptions)
    {
        _dbStatusRepo = dbStatusRepo;
        _famFeederOptions = famFeederOptions.Value;
    }

    public async Task<string> RunFeeder()
    {
        if (string.IsNullOrEmpty(_famFeederOptions.DbStatusAiCs))
        {
            return "AI connection-string for DbStatus not configured - exiting";
        }
        var metrics = await GetDatabaseStatus();
        LogMetricsToAi(metrics);
        return $"Finished logging {metrics.Count} metrics to AI";
    }

    private void LogMetricsToAi(List<MetricDto> metrics)
    {
        var configuration = new TelemetryConfiguration
        {
            ConnectionString = _famFeederOptions.DbStatusAiCs
        };
        var telemetryClient = new TelemetryClient(configuration);

        if (metrics.Any())
        {
            foreach (var metric in metrics)
            {
                var props = new Dictionary<string, string>
                {
                    { "UserName", metric.UserName },
                    { "Program", metric.Program },
                    { "SID", metric.Sid.ToString() },
                    { "Serial", metric.Serial.ToString() }
                };

                telemetryClient.TrackMetric(metric.Name, metric.Value, props);
            }
            telemetryClient.TrackTrace($"Tracked {metrics.Count()} metrics");
        }
        else
        {
            telemetryClient.TrackTrace($"No metrics found.");
        }

        telemetryClient.Flush();
        configuration.Dispose();
    }
    private async Task<List<MetricDto>> GetDatabaseStatus()
    {
        var metrics = await _dbStatusRepo.GetMetrics();
        return metrics;
    }
}