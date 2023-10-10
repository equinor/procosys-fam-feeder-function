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
        var (topicString, plant) = await DeserializeTopicAndPlant(req);

        log.LogInformation("Querying {Plant} for {TopicString}", plant, topicString);

        if (topicString == null || plant == null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }

        if (!TopicHelper.GetAllTopicsAsEnumerable().Contains(topicString))
        {
            return new BadRequestObjectResult("Please provide valid topic");
        }

        var param = new QueryParameters(plant, topicString);
        var instanceId = await orchestrationClient.StartNewAsync(nameof(TopicOrchestrator), param);
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    private static async Task<(string? topicString, string? plant)> DeserializeTopicAndPlant(HttpRequest req)
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