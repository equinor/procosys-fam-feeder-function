using System.IO;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Enum;

namespace FamFeederFunction
{
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

            await context.CallActivityAsync("RunFeeder",param);

            return context.InstanceId;
        }

        [FunctionName("RunFeeder")]
        public  async Task RunFeeder([ActivityTrigger] IDurableActivityContext context)
        {
             await _famFeederService.RunFeeder(context.GetInput<QueryParameters>());
        }

        [FunctionName("FamFeederFunction_HttpStart")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,ILogger log)
        {
            _logger = log;

            var (topicString, plant) = await Deserialize(req);

            if (topicString == null || plant == null)
            {
                return new BadRequestObjectResult("Please provide both plant and topic");
            }
            var parsed = TryParse(topicString, out PcsTopic topic);
            if (!parsed)
            {
                return new BadRequestObjectResult("Please provide valid topic");
            }
            var plants = await _famFeederService.GetAllPlants();
            if (!plants.Contains(plant))
            {
                return new BadRequestObjectResult("Please provide valid plant");
            }


            var param = new QueryParameters(plant, topic);

            //Explain
            var instanceId = await orchestrationClient.StartNewAsync("FamFeederFunction", param);
            return new OkObjectResult(instanceId);
        }

        private static async Task<(string topicString, string plant)> Deserialize(HttpRequest req)
        {
            string topicString = req.Query["PcsTopic"];
            string plant = req.Query["Facility"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            topicString ??= data?.PcsTopic;
            plant ??= data?.Facility;
            return (topicString, plant);
        }

        [FunctionName("GetStatus")]
        public static async Task<IActionResult> Status(
            [DurableClient] IDurableOrchestrationClient client,
            [HttpTrigger(AuthorizationLevel.Function,"get", Route = null)] string instanceId)
        {

            var statuses = await client.GetStatusAsync();
            var status = await client.GetStatusAsync(instanceId);
            if (status == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(status.LastUpdatedTime);
        }

        [FunctionName("TerminateInstance")]
        public static Task Terminate(
            [DurableClient] IDurableOrchestrationClient client,
            [HttpTrigger(AuthorizationLevel.Function,"get","instanceId", Route = null)] string instanceId)
        {
            return client.TerminateAsync(instanceId, "reason");
        }
    }
}
