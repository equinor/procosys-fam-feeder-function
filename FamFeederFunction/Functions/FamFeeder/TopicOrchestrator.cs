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

        var allPlants = new List<string>();
        allPlants.AddRange(param.Plants);

        foreach (var plant in param.Plants)
        {
            if (MultiPlantConstants.TryGetByMultiPlant(plant, out var validMultiPlants))
            {
                allPlants.Remove(plant);
                allPlants.AddRange(validMultiPlants.Except(allPlants));
            }
        }

        if (param.PcsTopics.Contains(PcsTopicConstants.WorkOrderCutoff, StringComparer.InvariantCultureIgnoreCase))
        {
            returnValue.AddRange(await RunMultiPlantWoCutoffOrchestration(context, allPlants));
        }

        foreach (var plant in allPlants)
        {
            var newParamList = param.PcsTopics
                .Where(s => s != PcsTopicConstants.WorkOrderCutoff)
                .Select(s =>
                    new QueryParameters(plant, s)).ToList();

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

        var newPlants = new List<string>();
        newPlants.AddRange(param.Plants);

        foreach (var plant in param.Plants)
        {
            if (MultiPlantConstants.TryGetByMultiPlant(plant, out var validMultiPlants))
            {
                newPlants.Remove(plant);
                newPlants.AddRange(validMultiPlants.Except(newPlants));
            }
        }

        return newPlants.Any(s => allPlants.Contains(s, StringComparer.InvariantCultureIgnoreCase));
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