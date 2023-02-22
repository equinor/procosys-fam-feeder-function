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
        var plant = context.GetInput<string>();

        if (MultiPlantConstants.TryGetByMultiPlant(plant, out var validMultiPlants))
        {
            return await RunMultiPlantOrchestration(context, validMultiPlants);
        }

        var plants = await context.CallActivityAsync<List<string>>(nameof(ValidatePlantActivity),context);
        if (!plants.Contains(plant))
        {
            return new List<string> { "Please provide a valid plant" };
        }

        var topics = GetAllTopicsAsEnumerable();

        var results = topics.Select(topic => context.CallActivityAsync<string>(nameof(TopicActivity), new QueryParameters(plant, topic)));

        var toReturn = await Task.WhenAll(results);
        return toReturn.ToList();
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
        var results = validMultiPlants.SelectMany(plant => GetAllTopicsAsEnumerable().Select(topic =>
            context.CallActivityAsync<string>(nameof(TopicActivity), new QueryParameters(plant, topic))));
        
        var toReturn = await Task.WhenAll(results);
        return toReturn.ToList();
    }
}