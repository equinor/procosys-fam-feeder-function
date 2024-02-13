using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using MoreLinq;
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

        if (!await HasAnyValidPlant(context))
        {
            return new List<string> { "Please provide one or more valid plants" };
        }

        if (param.PcsTopics.Contains(PcsTopicConstants.WorkOrderCutoff, StringComparer.InvariantCultureIgnoreCase))
        {
            returnValue.AddRange(await RunMultiPlantWoCutoffOrchestration(context, param.Plants));

            foreach (var plant in param.Plants)
            {
                if (MultiPlantConstants.TryGetByMultiPlant(plant, out var validMultiPlants))
                {
                    returnValue.AddRange(await RunMultiPlantWoCutoffOrchestration(context, validMultiPlants));
                }
            }
        }

        foreach (var plant in param.Plants)
        {
            var newParamList = param.PcsTopics
                .Where(s => s != PcsTopicConstants.WorkOrderCutoff)
                .Select(s =>
                    new QueryParameters(plant, s)).ToList();

            if (MultiPlantConstants.TryGetByMultiPlant(plant, out var validMultiPlants))
            {
                foreach (var newParam in newParamList)
                {
                    returnValue.AddRange(await RunMultiPlantOrchestration(context, validMultiPlants, newParam));
                }
            }

            foreach (var newParam in newParamList)
            {
                returnValue.Add(await context.CallActivityAsync<string>(nameof(TopicActivity), newParam));
            }
        }

        return returnValue;
    }

    //Check if there is one or more matching plants
    private static async Task<bool> HasAnyValidPlant(IDurableOrchestrationContext context)
    {
        var param = context.GetInput<QueryParameters>();
        var allPlants = await context.CallActivityAsync<List<string>>(nameof(GetValidPlantsActivity), null);

        return param.Plants.Any(s => allPlants.Contains(s, StringComparer.InvariantCultureIgnoreCase));
    }

    private static async Task<List<string>> RunMultiPlantOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants,
        QueryParameters param)
    {
        var results = validMultiPlants
            .Select(plant => new QueryParameters(new List<string> {plant}, param.PcsTopics))
            .Select(input => context.CallActivityAsync<string>(nameof(TopicActivity), input))
            .ToList();
        var finishedTasks = await Task.WhenAll(results);
        return finishedTasks.ToList();
    }
    
    private static async Task<List<string>> RunMultiPlantWoCutoffOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants)
    {
        var weekNumbers = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53" };
        var results = weekNumbers
            .SelectMany(weekNumber => validMultiPlants.Select(plant => (plant,weekNumber)))
            .Select(cutoffInput => ($"{cutoffInput.plant}({cutoffInput.weekNumber})", context.CallActivityAsync<string>(
                nameof(CutoffForWeekNumberActivity), cutoffInput))).ToList();
        var allFinishedTasks = await CustomStatusExtension.WhenAllWithStatusUpdate(context,results);
        return allFinishedTasks.ToList();
    }
}