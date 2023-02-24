using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FamFeederFunction.Functions.FamFeeder;

public class CutoffForMonthActivity
{
    private readonly IFamFeederService _famFeederService;

    public CutoffForMonthActivity(IFamFeederService famFeederService) 
        => _famFeederService = famFeederService;

    [FunctionName(nameof(CutoffForMonthActivity))]
    public async Task<string> RunWoCutoffActivity([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var (plant, month) = context.GetInput<(string, string)>();
        var result = await _famFeederService.WoCutoff(plant, month, logger);
        logger.LogDebug($"RunFeeder returned {result}");
        return result;
    }
}