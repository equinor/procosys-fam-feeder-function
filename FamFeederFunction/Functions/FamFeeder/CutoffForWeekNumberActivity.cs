using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FamFeederFunction.Functions.FamFeeder;

public class CutoffForWeekNumberActivity
{
    private readonly IFamFeederService _famFeederService;

    public CutoffForWeekNumberActivity(IFamFeederService famFeederService) 
        => _famFeederService = famFeederService;

    [FunctionName(nameof(CutoffForWeekNumberActivity))]
    public async Task<string> RunWoCutoffActivity([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var (plant, weekNumber) = context.GetInput<(string, string)>();
        var result = await _famFeederService.WoCutoff(plant, weekNumber, logger);
        logger.LogDebug($"RunFeeder returned {result}");
        return result;
    }
}