using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Misc;
using Core.Models;
using FamFeederFunction.Functions.FamFeeder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FamFeederFunction.Functions.SearchFeeder;

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
        var (topicsString, plantsString) = await Deserialize(req);

        log.LogInformation($"Querying {plantsString} for {topicsString}");

        if (topicsString is null || plantsString is null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }

        if (!TopicHttpTrigger.HasValidTopic(topicsString))
        {
            return new BadRequestObjectResult("Please provide one or more valid topics");
        }

        var plantsQuery = plantsString.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var topicsQuery = topicsString.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var newPlants = new List<string>();
        newPlants.AddRange(plantsQuery);

        foreach (var plant in plantsQuery)
        {
            if (MultiPlantConstants.TryGetByMultiPlant(plant, out var validMultiPlants))
            {
                newPlants.Remove(plant);
                newPlants.AddRange(validMultiPlants.Except(newPlants));
            }
        }

        var allPlants = await _searchFeederService.GetAllPlants();
        if (!newPlants.Any(s => allPlants.Contains(s, StringComparer.InvariantCultureIgnoreCase)))
        {
            return new BadRequestObjectResult("Please provide one or more valid plants");
        }

        var param = new QueryParameters(newPlants, new List<string>(topicsQuery));
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
        var results = new List<string>
            { await context.CallActivityAsync<string>("RunSearchFeeder", param) };
        return results;
    }

    [FunctionName("RunSearchFeeder")]
    public async Task<string> RunSearchFeeder([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var runFeeder = await _searchFeederService.RunFeeder(context.GetInput<QueryParameters>(), logger);
        logger.LogInformation($"RunSearchFeeder returned {runFeeder}");
        return runFeeder;
    }

    private static async Task<(string? topicString, string? plant)> Deserialize(HttpRequest req)
    {
        string? topicString = req.Query["PcsTopic"];
        string? plant = req.Query["Plant"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        topicString ??= data?.PcsTopic;
        plant ??= data?.Facility;
        return (topicString, plant);
    }
}