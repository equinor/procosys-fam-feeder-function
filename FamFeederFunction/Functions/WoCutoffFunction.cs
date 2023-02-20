using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FamFeederFunction.Functions;

public class WoCutoffFunction
{
    private readonly IFamFeederService _famFeederService;
    private readonly WoCutoffOptions _cutoffOptions;

    public WoCutoffFunction(IFamFeederService famFeederService, IOptions<WoCutoffOptions> cutoffOptions)
    {
        _famFeederService = famFeederService;
        _cutoffOptions = cutoffOptions.Value;
    }

    [FunctionName("RunCutoffForWeek")]
    public async Task<IActionResult> RunCutoffForWeek(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (cutoffWeek, plant) = await DeserializeCutoffWeekAndPlant(req);

        log.LogTrace($"Running feeder for wo cutoff for plant {plant} and week {cutoffWeek}");

        if (cutoffWeek is null || !int.TryParse(cutoffWeek, out _)) //Avoid sql injection
        {
            return new BadRequestObjectResult("Please specify CutoffWeek");
        }

        if (plant == null)
        {
            return new BadRequestObjectResult("Please provide plant");
        }

        var plants = await _famFeederService.GetAllPlants();
        if (!plants.Contains(plant))
        {
            return new BadRequestObjectResult("Please provide valid plant");
        }

        var enabledPlants = _cutoffOptions.EnabledPlants?.Split(",").ToList();
        if (enabledPlants == null || !enabledPlants.Contains(plant))
        {
            return new OkObjectResult($"{plant} not enabled in fff");
        }

        var instanceId = await orchestrationClient.StartNewAsync("CutoffForWeekOrchestration", null, (cutoffWeek, plant));
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    [FunctionName("CutoffForWeekOrchestration")]
    public static async Task<string> CutoffForWeekOrchestration(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
    {
        var (cutoffWeek, plant) = context.GetInput<(string, string)>();
        logger.LogDebug("RunCutoffForWeekActivity");
        return await context.CallActivityAsync<string>("RunCutoffForWeekActivity", (cutoffWeek, plant));
    }

    [FunctionName("RunCutoffForWeekActivity")]
    public async Task<string> RunCutoffForWeekActivity(
        [ActivityTrigger] IDurableActivityContext context,
        ILogger logger)
    {
        var (cutoffWeek, plant) = context.GetInput<(string, string)>();
        var result = await _famFeederService.RunForCutoffWeek(cutoffWeek, plant, logger);
        logger.LogDebug($"RunCutoffForWeekActivity returned {result}");
        return result;
    }

    private static async Task<(string topicString, string plant)> DeserializeCutoffWeekAndPlant(HttpRequest req)
    {
        string cutoffWeek = req.Query["CutoffWeek"];
        string plant = req.Query["Plant"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        cutoffWeek ??= data?.CutoffWeek;
        plant ??= data?.Facility;
        return (cutoffWeek, plant);
    }
}