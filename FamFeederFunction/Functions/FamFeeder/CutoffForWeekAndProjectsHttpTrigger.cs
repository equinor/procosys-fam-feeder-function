using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FamFeederFunction.Functions.FamFeeder;

public class CutoffForWeekAndProjectsHttpTrigger
{
      [FunctionName("RunCutoffForWeekAndProjectIds")]
    public async Task<IActionResult> RunCutoffForWeekAndProjectIds(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (cutoffWeek, plant,projectIds) = await DeserializeCutoffParameters(req);

        log.LogTrace("Running feeder for wo cutoff for plant {Plant} and week {CutoffWeek}", plant, cutoffWeek);

        if (cutoffWeek is null || !int.TryParse(cutoffWeek, out _)) //Avoid sql injection
        {
            return new BadRequestObjectResult("Please specify CutoffWeek");
        }

        if (plant is null)
        {
            return new BadRequestObjectResult("Please provide plant");
        }
        if (projectIds is null)
        {
            return new BadRequestObjectResult("Please provide projectIds");
        }

        MultiPlantConstants.TryGetByMultiPlant("ALL_ACCEPTED", out var enabledPlants);
        
        if (!enabledPlants.Contains(plant))
        {
            return new OkObjectResult($"{plant} not enabled");
        }

        var instanceId = await orchestrationClient.StartNewAsync(nameof(CutoffForWeekAndProjectIdsOrchestration), null, (cutoffWeek, plant));
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    [FunctionName(nameof(CutoffForWeekAndProjectIdsOrchestration))]
    public static async Task<string> CutoffForWeekAndProjectIdsOrchestration(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
    {
        var (cutoffWeek, plant,projectIds) = context.GetInput<(string, string, IEnumerable<long>)>();

        var allValidPlants = await context.CallActivityAsync<List<string>>(nameof(GetValidPlantsActivity), null);

        if (!allValidPlants.Contains(plant))
        {
            return "Please provide a valid plant";
        }

        logger.LogDebug("Run {Activity}", nameof(CutoffForWeekAndProjectsActivity));
        return await context.CallActivityAsync<string>(nameof(CutoffForWeekAndProjectsActivity), (cutoffWeek, plant, projectIds));
    }

    private static async Task<(string? topicString, string? plant, IEnumerable<long>? projectIds)> DeserializeCutoffParameters(HttpRequest req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        string? cutoffWeek = data?.CutoffWeek;
        string? plant = data?.Facility;
        IEnumerable<long>? projectIds = data?.ProjectIds.ToObject<List<long>>();
        return (cutoffWeek, plant, projectIds);
    }
}