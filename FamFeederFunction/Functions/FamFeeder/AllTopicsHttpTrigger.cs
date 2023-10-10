using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace FamFeederFunction.Functions.FamFeeder;

public static class AllTopicsHttpTrigger
{

    [FunctionName("RunAllTopicsHttpTrigger")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "Plant", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The Plant parameter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
    {
        string? plant = req.Query["Plant"];
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        plant ??= data?.Facility;

        log.LogTrace("Running feeder for all topics for plant {Plant}", plant);

        if (plant is null)
        {
            return new BadRequestObjectResult("Please provide both plant and topic");
        }

        var instanceId = await orchestrationClient.StartNewAsync(nameof(AllTopicsOrchestrator), null, plant);
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }
}