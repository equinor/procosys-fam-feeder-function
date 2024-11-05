using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Task = System.Threading.Tasks.Task;

namespace FamFeederFunction.Functions.FamFeeder;

public static class TopicOrchestrator
{
    [FunctionName(nameof(TopicOrchestrator))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var param = context.GetInput<QueryParameters>();
        var returnValue = new List<string>();

        if (!await HasValidPlants(context))
        {
            return new List<string> { "Please provide one or more valid plants" };
        }

        if (param.PcsTopic == PcsTopicConstants.WorkOrderCutoff)
        {
            if (MultiPlantConstants.TryGetByMultiPlant(param.Plants.First(), out var validMultiPlants))
            {
                returnValue.AddRange(await RunMultiPlantWoCutoffOrchestration(context, validMultiPlants));
            }
            else
            {
                returnValue.AddRange(await RunMultiPlantWoCutoffOrchestration(context, param.Plants));
            }
        }
        else //Not WorkOrderCutoff
        {
            if (param.Plants.Count > 1)
            {
                returnValue.AddRange(await RunMultiPlantOrchestration(context, param.Plants, param));
            }else if (MultiPlantConstants.TryGetByMultiPlant(param.Plants.First(), out var multiPlants))
            {
                returnValue.AddRange(await RunMultiPlantOrchestration(context, multiPlants, param));
            }
            else
            {
                returnValue.Add(await context.CallActivityAsync<string>(nameof(TopicActivity), param));
            }

        }
        return returnValue;
    }

    //Check if there is one or more matching plants
    private static async Task<bool> HasValidPlants(IDurableOrchestrationContext context)
    {
        var param = context.GetInput<QueryParameters>();
        if (param.Plants.Count == 1 && MultiPlantConstants.TryGetByMultiPlant(param.Plants.First(), out _))
        {
            return true;
        }
        
        var allPlantsFromDb = await context.CallActivityAsync<List<string>>(nameof(GetValidPlantsActivity), null);

        return param.Plants.All(plantFromInput => allPlantsFromDb.Contains(plantFromInput, StringComparer.InvariantCultureIgnoreCase));
    }

    private static async Task<List<string>> RunMultiPlantOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants,
        QueryParameters param)
    {
        var results = validMultiPlants
            .Select(plant => new QueryParameters(new List<string> {plant}, param))
            .Select(input => context.CallActivityAsync<string>(nameof(TopicActivity), input))
            .ToList();
        var finishedTasks = await Task.WhenAll(results);
        return finishedTasks.ToList();
    }
    
    private static async Task<List<string>> RunMultiPlantWoCutoffOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants)
    {
        var results = validMultiPlants
            .Select(plant => ($"{plant}", context.CallActivityAsync<string>(
                nameof(CutoffForWeekNumberActivity), plant))).ToList();
        var allFinishedTasks = await CustomStatusExtension.WhenAllWithStatusUpdate(context,results);
        return allFinishedTasks.ToList();
    }
}