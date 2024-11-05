using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FamFeederFunction.Functions.FamFeeder;

public class CutoffForWeekNumberActivity
{
    private readonly IFeederService _feederService;

    public CutoffForWeekNumberActivity(IFeederService feederService) 
        => _feederService = feederService;

    [FunctionName(nameof(CutoffForWeekNumberActivity))]
    public async Task<string> RunWoCutoffActivity([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var plant = context.GetInput<string>();
        var result = await _feederService.WoCutoff(plant, null, logger);
        logger.LogDebug($"RunFeeder returned {result}");
        return result;
    }
}