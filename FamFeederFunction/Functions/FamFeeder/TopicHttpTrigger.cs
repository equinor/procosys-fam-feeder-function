using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Misc;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FamFeederFunction.Functions.FamFeeder;

public class TopicHttpTrigger
{
    [FunctionName("FamFeederFunction_HttpStart")]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (topicsString, plants) = await DeserializeTopicAndPlant(req);
        log.LogInformation("Querying {Plant} for {TopicString}", plants, topicsString);

        if (topicsString is null || plants is null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }

        if (!HasValidTopic(topicsString))
        {
            return new BadRequestObjectResult("Please provide one or more valid topics");
        }

        var param = new QueryParameters(SplitList(plants), SplitList(topicsString));
        var instanceId = await orchestrationClient.StartNewAsync(nameof(TopicOrchestrator), param);
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }
    
    [FunctionName("CompletionTopicHttpTrigger")]
    public async Task<IActionResult> CompletionStart(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (topicsString, plants) = await DeserializeTopicAndPlant(req);
        log.LogInformation("Querying {Plant} for {TopicString}", plants, topicsString);

        if (topicsString is null || plants is null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }

        if (!HasValidTopic(topicsString))
        {
            return new BadRequestObjectResult("Please provide one or more valid topics");
        }

        var param = new QueryParameters(SplitList(plants), SplitList(topicsString),true);
        var instanceId = await orchestrationClient.StartNewAsync(nameof(TopicOrchestrator), param);
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    public static List<string> SplitList(string input)
    {
        return new List<string>(input.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool HasValidTopic(string topics)
    {
        var topicsQuery = SplitList(topics);
        return topicsQuery.Any(s =>
            TopicHelper.GetAllTopicsAsEnumerable().Contains(s, StringComparer.InvariantCultureIgnoreCase));
    }

    private static async Task<(string? topicsString, string? plants)> DeserializeTopicAndPlant(HttpRequest req)
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