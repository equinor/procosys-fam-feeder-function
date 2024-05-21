using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Core.Interfaces;

namespace FamFeederFunction.Functions.FamFeeder;

public  class CutoffForWeekActivity
{
    private readonly IFeederService _feederService;

    public CutoffForWeekActivity(IFeederService feederService) 
        => _feederService = feederService;

    [FunctionName(nameof(CutoffForWeekActivity))]
    public async Task<string> RunCutoffForWeekActivity(
        [ActivityTrigger] IDurableActivityContext context,
        ILogger logger)
    {
        var (cutoffWeek, plant) = context.GetInput<(string, string)>();
        var result = await _feederService.RunForCutoffWeek(cutoffWeek, plant, logger);
        logger.LogDebug("RunCutoffForWeekActivity returned {Result}", result);
        return result;
    }
}