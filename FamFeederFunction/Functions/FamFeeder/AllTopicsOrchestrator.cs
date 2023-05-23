using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Misc;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Enum = System.Enum;

namespace FamFeederFunction.Functions.FamFeeder;


public static class AllTopicsOrchestrator
{
    [FunctionName(nameof(AllTopicsOrchestrator))]
    public static async Task<List<string>> RunAllExceptCutoffOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        //Could either be a single plant ie: "PCS$JOHAN_CASTBERG" or a predetermined plantGroup ie. "OSEBERG" (keyword for group of the 4 Oseberg plants).
        //If its a plant group, we will run the orchestrator for all plants in the group
        var plantInput = context.GetInput<string>();

        if (MultiPlantConstants.TryGetByMultiPlant(plantInput, out var validSubPlants))
        {
            return await RunMultiPlantOrchestration(context, validSubPlants);
        }

        var allValidPlants = await context.CallActivityAsync<List<string>>(nameof(ValidatePlantActivity),context);
        if (!allValidPlants.Contains(plantInput))
        {
            return new List<string> { "Please provide a valid plant" };
        }

        IEnumerable<string> allTopics = TopicHelper.GetAllTopicsAsEnumerable();

        IEnumerable<(string statusInput, Task<string>)> tasksAndParams = allTopics.Select(topic =>
        {
            var statusInput = $"{plantInput}({topic})";
            return (statusInput, context.CallActivityAsync<string>(nameof(TopicActivity), new QueryParameters(plantInput, topic)));
        });

        List<string> allTaskResults = await CustomStatusExtension.WhenAllWithStatusUpdate(context, tasksAndParams.ToList());
        return allTaskResults;
    }

    private static async Task<List<string>> RunMultiPlantOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants)
    {
        IEnumerable<(string statusInput, Task<string>)> tasksAndCustomStatusInput = validMultiPlants.SelectMany(plant => TopicHelper.GetAllTopicsAsEnumerable().Select(topic =>
        {
            var statusInput = $"{plant}({topic})";
            return (statusInput, context.CallActivityAsync<string>(nameof(TopicActivity), new QueryParameters(plant,topic)));
        }));

        List<string> allFinishedTasks = await CustomStatusExtension.WhenAllWithStatusUpdate(context, tasksAndCustomStatusInput.ToList());
        return allFinishedTasks;
    }
}