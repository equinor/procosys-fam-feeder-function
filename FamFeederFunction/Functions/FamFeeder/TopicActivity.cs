using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Core.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Core.Interfaces;

namespace FamFeederFunction.Functions.FamFeeder;

public class TopicActivity
{
    private readonly IFamFeederService _famFeederService;

    public TopicActivity(IFamFeederService famFeederService)
    {
        _famFeederService = famFeederService;
    }

    [FunctionName(nameof(TopicActivity))]
    public async Task<string> RunFeeder([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var runFeeder = await _famFeederService.RunFeeder(context.GetInput<QueryParameters>(), logger);
        logger.LogInformation($"RunFeeder returned {runFeeder}");
        return runFeeder;
    }
}