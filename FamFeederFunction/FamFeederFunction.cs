using System.IO;
using System.Linq;
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
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Enum;

namespace FamFeederFunction
{
    public class FamFeederFunction
    {
        private readonly IFamFeederService _famFeederService;
        private const string Value = "value";
        private ILogger _logger;

        public FamFeederFunction(IFamFeederService famFeederService)
        {
            _famFeederService = famFeederService;
        }

        [FunctionName("FamFeederFunction")]
        public async Task<string> RunOrchestrator(
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

            string topicString = req.Query["PcsTopic"];
            string facility = req.Query["Facility"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            topicString ??= data?.PcsTopic;
            facility ??= data?.Facility;
            if (topicString == null || facility == null)
            {
                return new BadRequestObjectResult("responseMessage");
            }

            var parsed = TryParse(topicString, out PcsTopic topic);
            if (!parsed)
            {
                return new BadRequestObjectResult("responseMessage");
            }

            var param = new QueryParameters(facility, topic);

            //Explain
            var instanceId = await orchestrationClient.StartNewAsync("FamFeederFunction", param);
            return new OkObjectResult(instanceId);
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
            [HttpTrigger(AuthorizationLevel.Function,"get", Route = null)] string instanceId)
        {
            string reason = "Found a bug";
            return client.TerminateAsync(instanceId, reason);
        }

        
        [FunctionName("TerminateAll")]
        public static async Task TerminateAll(
            [DurableClient] IDurableOrchestrationClient client,
            [HttpTrigger(AuthorizationLevel.Function,"get", Route = null)] string instanceId)
        {
            string reason = "Stop everything";
            var statuses = await client.GetStatusAsync();

            statuses.ForEach( s =>
            {
                client.TerminateAsync(s.InstanceId, reason);
            });
           
        }

    }
}
