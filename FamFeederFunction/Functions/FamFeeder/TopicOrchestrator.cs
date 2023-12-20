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

        if (MultiPlantConstants.TryGetByMultiPlant(param.Plant,out var validMultiPlants))
        {
            if(param.PcsTopic == PcsTopicConstants.WorkOrderCutoff)
            {
                return await RunMultiPlantWoCutoffOrchestration(context, validMultiPlants, param);
            }

            return await RunMultiPlantOrchestration(context, validMultiPlants, param);
        }
      
        var plants = await context.CallActivityAsync<List<string>>(nameof(GetValidPlantsActivity),null);
        if (!plants.Contains(param.Plant))
        {
            return new List<string> { "Please provide a valid plant" };
        }

        if (param.PcsTopic == PcsTopicConstants.WorkOrderCutoff)
        {
            return await RunWoCutoffOrchestration(context, param);
        }
        var singleReturn = await context.CallActivityAsync<string>(nameof(TopicActivity), param);
        return new List<string> { singleReturn };
    }

    private static async Task<List<string>> RunMultiPlantOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants,
        QueryParameters param)
    {
        var results = validMultiPlants
            .Select(plant => new QueryParameters(plant, param.PcsTopic))
            .Select(input => context.CallActivityAsync<string>(nameof(TopicActivity), input))
            .ToList();
        var finishedTasks = await Task.WhenAll(results);
        return finishedTasks.ToList();
    }
    
    private static async Task<List<string>> RunMultiPlantWoCutoffOrchestration(IDurableOrchestrationContext context, IEnumerable<string> validMultiPlants,
        QueryParameters param)
    {
        var weekNumbers = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53" };
        var results = weekNumbers
            .SelectMany(weekNumber => validMultiPlants.Select(plant => (plant,weekNumber)))
            .Select(cutoffInput => ($"{cutoffInput.plant}({cutoffInput.weekNumber})", context.CallActivityAsync<string>(
                nameof(CutoffForWeekNumberActivity), cutoffInput))).ToList();
        var allFinishedTasks = await CustomStatusExtension.WhenAllWithStatusUpdate(context,results);
        return allFinishedTasks.ToList();
    }

    private static async Task<List<string>> RunWoCutoffOrchestration(IDurableOrchestrationContext context, QueryParameters param)
    {
        var weekNumbers = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53" };
        var results = weekNumbers
            .Select(m => (param.Plant, m))
            .Select(cutoffInput => ($"{cutoffInput.Plant}({cutoffInput.m})", context.CallActivityAsync<string>(
                nameof(CutoffForWeekNumberActivity), cutoffInput))).ToList();
        var allFinishedTasks = await CustomStatusExtension.WhenAllWithStatusUpdate(context,results);
        return allFinishedTasks.ToList();
    }
}