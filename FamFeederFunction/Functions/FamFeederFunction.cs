using System.Collections.Generic;
using System.IO;
using System.Linq;
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
namespace FamFeederFunction.Functions;

public class FamFeederFunction
{
    private readonly IFamFeederService _famFeederService;

    public FamFeederFunction(IFamFeederService famFeederService)
    {
        _famFeederService = famFeederService;
    }

    //[FunctionName("FamFeederFunction_HttpStart")]
    //public async Task<IActionResult> HttpStart(
    //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
    //    HttpRequest req,
    //    [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    //{
    //    var (topicString, plant) = await DeserializeTopicAndPlant(req);

    //    log.LogInformation($"Querying {plant} for {topicString}");

    //    if (topicString == null || plant == null)
    //    {
    //        return new BadRequestObjectResult("Please provide both plant and topic");
    //    }

    //    if (!TryParse(topicString, out PcsTopic _))
    //    {
    //        return new BadRequestObjectResult("Please provide valid topic");
    //    }

    //    var plants = await _famFeederService.GetAllPlants();
    //    if (!plants.Contains(plant))
    //    {
    //        return new BadRequestObjectResult("Please provide valid plant");
    //    }

    //    var param = new QueryParameters(plant, topicString);
    //    var instanceId = await orchestrationClient.StartNewAsync("FamFeederFunction", param);
    //    return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    //}

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
    public static async  Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var param = context.GetInput<QueryParameters>();
        if (param.PcsTopic == PcsTopic.WorkOrderCutoff.ToString())
        {
            return await RunWoCutoffOrchestration(context, param);
        }
        var singleReturn = await context.CallActivityAsync<string>("RunFeeder", param);
        return new List<string> {singleReturn};
    }

    private static async Task<List<string>> RunWoCutoffOrchestration(IDurableOrchestrationContext context, QueryParameters param)
    {
        var months = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
        var results = months
            .Select(m => (param.Plant, m))
            .Select(cutoffInput => context.CallActivityAsync<string>(
                "RunWoCutoffActivity", cutoffInput)).ToList();

        var statuses = new List<string>();
        results.ForEach(r => r.ContinueWith(str =>
        {
            statuses.Add(str.IsFaulted ? str.Exception?.Message : str.Result);
            context.SetCustomStatus(statuses);
        }));

        var toReturn = await Task.WhenAll(results);
        return toReturn.ToList();
    }

    [FunctionName("RunAllExceptCutoff")]
    public static async Task<List<string>> RunAllExceptCutoffOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var plant = context.GetInput<string>();
        var results = new List<Task<string>>();
        
        var topics = new List<PcsTopic> { PcsTopic.Action,PcsTopic.CommPkgTask,PcsTopic.Task,PcsTopic.CommPkg,PcsTopic.McPkg,PcsTopic.Project,PcsTopic.Responsible,PcsTopic.Tag,
            PcsTopic.TagFunction,PcsTopic.PunchListItem,PcsTopic.Library,PcsTopic.WorkOrder,PcsTopic.Checklist,PcsTopic.Milestone,PcsTopic.WoChecklist,PcsTopic.SWCR,PcsTopic.SWCRSignature,PcsTopic.PipingRevision,
            PcsTopic.WoMaterial,PcsTopic.WoMilestone,PcsTopic.Stock,PcsTopic.CommPkgOperation,PcsTopic.PipingSpool,PcsTopic.LoopContent,PcsTopic.Query,PcsTopic.QuerySignature,PcsTopic.CallOff,
            PcsTopic.CommPkgQuery,PcsTopic.HeatTrace
            }.Select(t => t.ToString());

        foreach (var topic in topics)
        {
            var activities =  context.CallActivityAsync<string>("RunFeeder", new QueryParameters(plant, topic));
            results.Add(activities);
        }

        var statuses = new List<string>();
        results.ForEach(r => r.ContinueWith(str =>
        {
            statuses.Add(str.IsFaulted ? str.Exception?.Message : str.Result);
            context.SetCustomStatus(statuses);
        }));
        var toReturn = await Task.WhenAll(results);
        return toReturn.ToList();
    }

    [FunctionName("RunWoCutoffActivity")]
    public async Task<string> RunWoCutoffActivity([ActivityTrigger] IDurableActivityContext context, ILogger logger)
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
}