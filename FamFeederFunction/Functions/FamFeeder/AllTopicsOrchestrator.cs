using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

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

        var allTopics = GetAllTopicsAsEnumerable();

        var tasksAndParams = allTopics.Select(topic =>
        {
            var statusInput = $"{plantInput}({topic})";
            return (statusInput, context.CallActivityAsync<string>(nameof(TopicActivity), new QueryParameters(plantInput, topic)));
        });

        var allTaskResults = await CustomStatusExtension.WhenAllWithStatusUpdate(context, tasksAndParams.ToList());
        return allTaskResults;
    }

    private static IEnumerable<string> GetAllTopicsAsEnumerable()
    {
        return new List<PcsTopic> { PcsTopic.Action,PcsTopic.CommPkgTask,PcsTopic.Task,PcsTopic.CommPkg,PcsTopic.McPkg,PcsTopic.Project,PcsTopic.Responsible,PcsTopic.Tag,
            PcsTopic.TagFunction,PcsTopic.PunchListItem,PcsTopic.Library,PcsTopic.WorkOrder,PcsTopic.Checklist,PcsTopic.Milestone,PcsTopic.WoChecklist,PcsTopic.SWCR,PcsTopic.SWCRSignature,PcsTopic.PipingRevision,
            PcsTopic.WoMaterial,PcsTopic.WoMilestone,PcsTopic.Stock,PcsTopic.CommPkgOperation,PcsTopic.PipingSpool,PcsTopic.LoopContent,PcsTopic.Query,PcsTopic.QuerySignature,PcsTopic.CallOff,
            PcsTopic.CommPkgQuery,PcsTopic.HeatTrace
        }.Select(t => t.ToString());
    }

    private static async Task<List<string>> RunMultiPlantOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants)
    {
        var tasksAndCustomStatusInput = validMultiPlants.SelectMany(plant => GetAllTopicsAsEnumerable().Select(topic =>
        {
            var statusInput = $"{plant}({topic})";
            return (statusInput, context.CallActivityAsync<string>(nameof(TopicActivity), new QueryParameters(plant,topic)));
        }));

        var allFinishedTasks = await CustomStatusExtension.WhenAllWithStatusUpdate(context, tasksAndCustomStatusInput.ToList());
        return allFinishedTasks;
    }
}