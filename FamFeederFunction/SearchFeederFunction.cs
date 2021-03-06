using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Enum;
namespace SearchFeederFunction;

public class SearchFeederFunction
{
    private readonly ISearchFeederService _searchFeederService;

    public SearchFeederFunction(ISearchFeederService searchFeederService)
    {
        _searchFeederService = searchFeederService;
    }

    [FunctionName("SearchFeederFunction_HttpStart")]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (topicString, plant) = await Deserialize(req);

        log.LogInformation($"Querying {plant} for {topicString}");

        if (topicString == null || plant == null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }

        var parsed = TryParse(topicString, out PcsTopic topic);
        if (!parsed)
        {
            return new BadRequestObjectResult("Please provide valid topic");
        }

        var plants = await _searchFeederService.GetAllPlants();
        if (!plants.Contains(plant))
        {
            return new BadRequestObjectResult("Please provide valid plant");
        }

        var param = new QueryParameters(plant, topic);
        var instanceId = await orchestrationClient.StartNewAsync("SearchFeederFunction", param);
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    [FunctionName("GetSearchStatuses")]
    public static async Task<IActionResult> SearchStatuses(
        [DurableClient] IDurableOrchestrationClient client,
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest request)
    {
        var statuses = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition(), CancellationToken.None);
        return new OkObjectResult(statuses);
    }

    [FunctionName("SearchFeederFunction")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var param = context.GetInput<QueryParameters>();
        var results = new List<string>();
        results.Add(await context.CallActivityAsync<string>("RunSearchFeeder", param));
        return results;
    }
    
    [FunctionName("RunSearchFeeder")]
    public async Task<string> RunSearchFeeder([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var runFeeder = await _searchFeederService.RunFeeder(context.GetInput<QueryParameters>(), logger);
        logger.LogInformation($"RunSearchFeeder returned {runFeeder}");
        return runFeeder;
    }

    private static async Task<(string topicString, string plant)> Deserialize(HttpRequest req)
    {
        string topicString = req.Query["PcsTopic"];
        string plant = req.Query["Plant"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        topicString ??= data?.PcsTopic;
        plant ??= data?.Facility;
        return (topicString, plant);
    }
}