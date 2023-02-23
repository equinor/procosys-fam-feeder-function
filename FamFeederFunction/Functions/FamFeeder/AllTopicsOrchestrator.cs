using System;
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
        var results = validMultiPlants.SelectMany(plant => GetAllTopicsAsEnumerable().Select(topic 
            => context.CallActivityAsync<string>(nameof(TopicActivity), new QueryParameters(plant, topic))));

        var toReturn = await WhenAllWithStatusUpdate(context, results.ToList());
        //var toReturn = await Task.WhenAll(results);
        return toReturn;
    }


    /// <summary>
    /// https://joonasw.net/view/track-activity-and-sub-orchestrator-progress-in-azure-durable-functions-orchestrators
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tasks"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="AggregateException"></exception>
    [Deterministic]
    private static async Task<List<string>> WhenAllWithStatusUpdate(
    IDurableOrchestrationContext context,
    List<Task<string>> tasks)
    {
        var activityStatuses = new string[tasks.Count];
        var doneActivityCount = 0;
        context.SetCustomStatus(activityStatuses.Select(s => s.ToString()));

        while (doneActivityCount < tasks.Count)
        {
            // Wait for one of the not done tasks to complete
            var notDoneTasks = tasks.Where((t, i) => activityStatuses[i] == "Started");
            var doneTask = await Task.WhenAny(notDoneTasks);

            // Find which one completed
            var doneTaskIndex = tasks.FindIndex(t => ReferenceEquals(t, doneTask));
            // Sanity check
            if (doneTaskIndex < 0 || activityStatuses[doneTaskIndex] != "Started")
            {
                throw new Exception(
                    "Something went wrong, completed task not found or it was already completed");
            }

            activityStatuses[doneTaskIndex] = GetActivityStatusFromTask(doneTask);
            doneActivityCount++;

            if (!context.IsReplaying)
            {
                // Only log and update status when not replaying
                //logger.LogInformation(
                //    "Task {Index} completed, status {Status}, {Count} tasks done",
                //    doneTaskIndex,
                //    activityStatuses[doneTaskIndex],
                //    doneActivityCount);
                context.SetCustomStatus(activityStatuses.Select(s => s.ToString()));
            }
        }

        var failedTasks = tasks.Where(t => t.Exception != null).ToList();
        if (failedTasks.Count > 0)
        {
            throw new AggregateException(
                "One or more operations failed", failedTasks.Select(t => t.Exception));
        }
        var toReturn = await Task.WhenAll(tasks);
        return toReturn.ToList();
    }

    [Deterministic]
    private static string GetActivityStatusFromTask(Task<string> task)
    {
        return task.Status switch
        {
            TaskStatus.Created => "Started",
            TaskStatus.WaitingForActivation => "Started",
            TaskStatus.WaitingToRun => "Started",
            TaskStatus.Running => "Started",
            TaskStatus.WaitingForChildrenToComplete =>" Started",
            TaskStatus.RanToCompletion => task.Result,
            TaskStatus.Canceled => "Failed",
            TaskStatus.Faulted => "Failed",
            _ => throw new NotImplementedException(),
        };
    }

}