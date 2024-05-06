using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FamFeederFunction.Functions.FamFeeder;

public class CutoffForWeekAndProjectsActivity
{
    private readonly IFeederService _feederService;

    public CutoffForWeekAndProjectsActivity(IFeederService feederService) 
        => _feederService = feederService;

    [FunctionName(nameof(CutoffForWeekAndProjectsActivity))]
    public async Task<string> RunCutoffForWeekAndProjectsActivity(
        [ActivityTrigger] IDurableActivityContext context,
        ILogger logger)
    {
        var (cutoffWeek, plant,projectIds) = context.GetInput<(string, string, IEnumerable<long>)>();
        var result = await _feederService.RunForCutoffWeek(cutoffWeek, projectIds, plant,logger);
        logger.LogDebug("RunCutoffForWeekActivity returned {Result}", result);
        return result;
    }
}