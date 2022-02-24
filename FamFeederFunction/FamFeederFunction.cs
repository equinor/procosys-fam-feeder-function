using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
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
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace FamFeederFunction;

public class FamFeederFunction
{
    private readonly IFamFeederService _famFeederService;
    private ILogger _logger;

    public FamFeederFunction(IFamFeederService famFeederService)
    {
        _famFeederService = famFeederService;
    }

    [FunctionName("FamFeederFunction")]
    public static async Task<string> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var param = context.GetInput<QueryParameters>();

        await context.CallActivityAsync("RunFeeder", param);

        return context.InstanceId;
    }

    [FunctionName("RunFeeder")]
    public async Task RunFeeder([ActivityTrigger] IDurableActivityContext context)
    {
        await _famFeederService.RunFeeder(context.GetInput<QueryParameters>());
    }

    [FunctionName("FamFeederFunction_HttpStart")]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        _logger = log;

        var (topicString, plant) = await Deserialize(req);

        _logger.LogInformation($"Querying {plant} for {topicString}");

        if (topicString == null || plant == null)
            return new BadRequestObjectResult("Please provide both plant and topic");
        var parsed = TryParse(topicString, out PcsTopic topic);
        if (!parsed) return new BadRequestObjectResult("Please provide valid topic");
        var plants = await _famFeederService.GetAllPlants();
        if (!plants.Contains(plant)) return new BadRequestObjectResult("Please provide valid plant");

        var param = new QueryParameters(plant, topic);


        //Explain
        var instanceId = await orchestrationClient.StartNewAsync("FamFeederFunction", param);
        return new OkObjectResult(instanceId);
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

    [FunctionName("GetStatuses")]
    public static async Task<IActionResult> Statuses(
        [DurableClient] IDurableOrchestrationClient client,
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest request,ILogger log,ExecutionContext context)
    {

        var binpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var rootpath = Path.GetFullPath(Path.Combine(binpath, ".."));
        log.LogInformation("rootpath? "+rootpath);
        log.LogInformation("appdir "+context.FunctionAppDirectory);

        GetWalletFileFromBlobStorage(rootpath, log);
       
        var statuses = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition(), CancellationToken.None);
        return new OkObjectResult(statuses);
    }

    private static async void GetWalletFileFromBlobStorage(string rootPath, ILogger log)
    {
        var connectionString = ConfigurationManager.AppSettings["BlobStorage:ConnectionString"];
        var blobContainerName = ConfigurationManager.AppSettings["BlobStorage:ContainerName"];
        var blobName = ConfigurationManager.AppSettings["BlobStorage:WalletFileName"];
        if (string.IsNullOrWhiteSpace(connectionString) 
            || string.IsNullOrWhiteSpace(blobContainerName) ||
            string.IsNullOrWhiteSpace(blobName)) return;
        //var p1 = Directory.GetParent(rootPath);
        //if (p1 == null) return;
        //var p2 = Directory.GetParent(p1.FullName);
        //if (p2 == null) return;

        var fileName = rootPath + @"/Users/test/wallet/cwallet.sso";
        log.LogInformation("Created wallet file at: " + fileName);
        var blobClient = new BlobClient(connectionString, blobContainerName, blobName);
        var response = await blobClient.DownloadAsync();
        await using var outputFileStream = new FileStream(fileName, FileMode.Create);
        await response.Value.Content.CopyToAsync(outputFileStream);
    }

    [FunctionName("GetStatus")]
    public static async Task<IActionResult> Status(
        [DurableClient] IDurableOrchestrationClient client,
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status/{instanceId}")]
        HttpRequest request, string instanceId)
    {
        var statuses = await client.GetStatusAsync(instanceId);
        return new OkObjectResult(statuses);
    }

    [FunctionName("TerminateInstance")]
    public static Task Terminate(
        [DurableClient] IDurableOrchestrationClient client,
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "TerminateInstance/{instanceId}")]
        HttpRequest request, string instanceId)
    {
        return client.TerminateAsync(instanceId, "reason");
    }

}