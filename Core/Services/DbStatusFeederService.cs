using Core.Interfaces;
using Core.Models.DbStatus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Services;

public class DbStatusFeederService : IDbStatusFeederService
{
    private ILogger? _logger;
    private readonly FamFeederOptions _famFeederOptions;
    private readonly IDbStatusRepository _dbStatusRepo;

    public DbStatusFeederService(
        IDbStatusRepository dbStatusRepo,
        IOptions<FamFeederOptions> famFeederOptions)
    {
        _dbStatusRepo = dbStatusRepo;
        _famFeederOptions = famFeederOptions.Value;
    }

    public async Task<string> RunFeeder(ILogger logger)
    {
        if (_famFeederOptions.DbStatusAiCs == null || _famFeederOptions.DbStatusAiCs == "")
        {
            return "AI connection-string for DbStatus not configured - exiting";
        }

        _logger = logger;
      
        var metrics = await GetSbStatus();

        if (metrics != null)
        {
            LogMetricsToAi(metrics);
            return $"Finished logging {metrics.Count} metrics to AI";
        }
        else
        {
            _logger.LogInformation("found no items");
            return "Found no items";
        }
    }


    private void LogMetricsToAi(IEnumerable<MetricDto> metrics)
    {
        var configuration = new TelemetryConfiguration
        {
            ConnectionString = _famFeederOptions.DbStatusAiCs
        };
        var _telemetryClient = new TelemetryClient(configuration);

        if (metrics.Any())
        {
            foreach (var metric in metrics)
            {
                var props = new Dictionary<string, string>();
                props.Add("UserName", metric.UseName);
                props.Add("Program", metric.Program);
                props.Add("SID", metric.Sid.ToString());
                props.Add("Serial", metric.Serial.ToString());

                _telemetryClient.TrackMetric(metric.Name, metric.Value, props);
            }
            _telemetryClient.TrackTrace($"Tracked {metrics.Count()} metrics");
        }
        else
        {
            _telemetryClient.TrackTrace($"No metrics found.");
        }

        _telemetryClient.Flush();
        configuration.Dispose();
    }
    private async Task<List<MetricDto>> GetSbStatus()
    {
        var metrics = await _dbStatusRepo.GetMetrics();
        return metrics;
    }
}