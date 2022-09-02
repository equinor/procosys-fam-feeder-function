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
namespace FamFeederFunction;

public class FamFeederFunction
{
    private readonly IFamFeederService _famFeederService;

    public FamFeederFunction(IFamFeederService famFeederService)
    {
        _famFeederService = famFeederService;
    }

    [FunctionName("FamFeederFunction_HttpStart")]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (topicString, plant) = await DeserializeTopicAndPlant(req);

        log.LogInformation($"Querying {plant} for {topicString}");

        if (topicString == null || plant == null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }

        if (!TryParse(topicString, out PcsTopic topic))
        {
            return new BadRequestObjectResult("Please provide valid topic");
        }

        var plants = await _famFeederService.GetAllPlants();
        if (!plants.Contains(plant))
        {
            return new BadRequestObjectResult("Please provide valid plant");
        }

        var param = new QueryParameters(plant, topic);
        var instanceId = await orchestrationClient.StartNewAsync("FamFeederFunction", param);
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    [FunctionName("FamFeederFunction_RunAll")]
    public async Task<IActionResult> RunAllHttpTrigger(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        string plant = req.Query["Plant"];
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        plant ??= data?.Facility;

        log.LogTrace($"Running feeder for all topics for plant {plant}");

        if (plant == null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }
        var plants = await _famFeederService.GetAllPlants();
        if (!plants.Contains(plant))
        {
            return new BadRequestObjectResult("Please provide valid plant");
        }

        var instanceId = await orchestrationClient.StartNewAsync("RunAllExceptCutoff", null, plant);
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    [FunctionName("FamFeederFunction")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var param = context.GetInput<QueryParameters>();
        var results = new List<string>();
        if (param.PcsTopic == PcsTopic.WorkOrderCutoff)
        {
            var months = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            foreach (var cutoffInput in months.Select(m => (param.Plant, m)))
            {
                results.Add(await context.CallActivityAsync<string>("RunWoCutoffFeeder", cutoffInput));
            }
            return results;
        }

        results.Add(await context.CallActivityAsync<string>("RunFeeder", param));
        return results;
    }

    [FunctionName("RunAllExceptCutoff")]
    public static async Task<List<string>> RunAllExceptCutoffOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var plant = context.GetInput<string>();
        var results = new List<string>();

        var topics = new List<PcsTopic> { PcsTopic.CommPkg, PcsTopic.McPkg,PcsTopic.Tag,PcsTopic.Milestone,
            PcsTopic.Checklist,PcsTopic.WorkOrder,PcsTopic.WoChecklist, PcsTopic.PipingRevision, PcsTopic.SWCR,
            PcsTopic.SWCRSignature, PcsTopic.Stock, PcsTopic.WoMaterial, PcsTopic.WoMilestone, PcsTopic.Library,
            PcsTopic.Responsible, PcsTopic.Query,PcsTopic.QuerySignature,PcsTopic.LoopContent,
            PcsTopic.CommPkgOperation};
        foreach (var topic in topics)
        {
            results.Add(await context.CallActivityAsync<string>("RunFeeder", new QueryParameters(plant, topic)));
        }
        return results;
    }

    [FunctionName("RunWoCutoffFeeder")]
    public async Task<string> RunWoCutoffFeeder([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var (plant, month) = context.GetInput<(string, string)>();
        var result = await _famFeederService.WoCutoff(plant, month, logger);
        logger.LogDebug($"RunFeeder returned {result}");
        return result;
    }

  

    [FunctionName("RunFeeder")]
    public async Task<string> RunFeeder([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var runFeeder = await _famFeederService.RunFeeder(context.GetInput<QueryParameters>(), logger);
        logger.LogInformation($"RunFeeder returned {runFeeder}");
        return runFeeder;
    }

    [FunctionName("FamFeederFunction_RunCutoffForWeek")]
    public async Task<IActionResult> RunCutoffForWeek(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        var (cutoffWeek, plant) = await DeserializeCutoffWeekAndPlant(req);       

        log.LogTrace($"Running feeder for wocutoff for plant {plant} and week {cutoffWeek}");

        if(!(cutoffWeek == "202234" || cutoffWeek  == "202235"))
        {
            return new BadRequestObjectResult("Jone, bruk rett CutoffWeek: 202235. ");
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

        var instanceId = await orchestrationClient.StartNewAsync("RunWoCutoffFeederForCutoffWeek", null, new { cutoffWeek, plant });
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }


    [FunctionName("RunWoCutoffFeederForCutoffWeek")]
    public async Task<string> RunWoCutoffFeederForCutoffWeek([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
    //public async Task<string> RunWoCutoffFeederForCutoffWeek([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var (cutoffWeek, plant) = context.GetInput<(string, string)>();
        var result = await _famFeederService.RunForCutoffWeek(cutoffWeek, plant, logger);
        logger.LogDebug($"RunWoCutoffFeederForCutoffWeek returned {result}");
        return result;
    }

    private static async Task<(string topicString, string plant)> DeserializeTopicAndPlant(HttpRequest req)
    {
        string topicString = req.Query["PcsTopic"];
        string plant = req.Query["Plant"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        topicString ??= data?.PcsTopic;
        plant ??= data?.Facility;
        return (topicString, plant);
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