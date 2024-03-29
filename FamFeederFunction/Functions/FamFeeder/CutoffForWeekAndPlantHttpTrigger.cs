using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FamFeederFunction.Functions.FamFeeder;

public class CutoffForWeekAndPlantHttpTrigger
{
    private readonly FamFeederOptions _famFeederOptions;

    public CutoffForWeekAndPlantHttpTrigger(IOptions<FamFeederOptions> famFeederOptions)
    {
        _famFeederOptions = famFeederOptions.Value;
    }

    [FunctionName("RunCutoffForWeek")]
    public async Task<IActionResult> RunCutoffForWeek(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (cutoffWeek, plant) = await DeserializeCutoffWeekAndPlant(req);

        log.LogTrace("Running feeder for wo cutoff for plant {Plant} and week {CutoffWeek}", plant, cutoffWeek);

        if (cutoffWeek is null || !int.TryParse(cutoffWeek, out _)) //Avoid sql injection
        {
            return new BadRequestObjectResult("Please specify CutoffWeek");
        }

        if (plant is null)
        {
            return new BadRequestObjectResult("Please provide plant");
        }
        if (!_famFeederOptions.FamFeederPlantFilterList.Contains(plant))
        {
            return new OkObjectResult($"{plant} not enabled in fff");
        }

        var instanceId = await orchestrationClient.StartNewAsync(nameof(CutoffForWeekOrchestration), null, (cutoffWeek, plant));
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    [FunctionName(nameof(CutoffForWeekOrchestration))]
    public static async Task<string> CutoffForWeekOrchestration(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
    {
        var (cutoffWeek, plant) = context.GetInput<(string, string)>();

        var allValidPlants = await context.CallActivityAsync<List<string>>(nameof(GetValidPlantsActivity), null);

        if (!allValidPlants.Contains(plant))
        {
            return "Please provide a valid plant";
        }

        logger.LogDebug("RunCutoffForWeekActivity");
        return await context.CallActivityAsync<string>(nameof(CutoffForWeekActivity), (cutoffWeek, plant));
    }

    private static async Task<(string? topicString, string? plant)> DeserializeCutoffWeekAndPlant(HttpRequest req)
    {
        string? cutoffWeek = req.Query["CutoffWeek"];
        string? plant = req.Query["Plant"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        cutoffWeek ??= data?.CutoffWeek;
        plant ??= data?.Facility;
        return (cutoffWeek, plant);
    }
}