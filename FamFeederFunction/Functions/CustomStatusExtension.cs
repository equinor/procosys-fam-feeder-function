using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FamFeederFunction.Functions;

public static class CustomStatusExtension
{
    /// <summary>
    /// Update status of tasks continuously.
    /// Loops through all pending tasks and updates status of done tasks until all tasks are completed.
    /// 
    /// Pattern found here
    /// https://joonasw.net/view/track-activity-and-sub-orchestrator-progress-in-azure-durable-functions-orchestrators
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tasks"></param>
    /// <returns>The combined result of all finished tasks</returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="AggregateException"></exception>
    [Deterministic]
    public static async Task<List<string>> WhenAllWithStatusUpdate(
        IDurableOrchestrationContext context,
        List<(string statusInput, Task<string> task)> tasks)
    {
        List<(TaskStatus TaskStatus, string custumStatus)> activityStatuses = tasks.Select(GetActivityStatusFromTaskAndParam).ToList();

        //Durable functions custom status takes a maximum payload of 16KB. For very large data sets we will not be updating the status continuously. 
        var payloadSizeInKb = (int)(Encoding.Unicode.GetByteCount(JsonSerializer.Serialize(activityStatuses.Select(s => s.custumStatus))) / 1024.0);
        var tooLargeSize = payloadSizeInKb > 14;
        if (tooLargeSize)
        {
            context.SetCustomStatus($"No custom status update because the payload would be too large. {tasks.Count} tasks");
            var allFinishedTasksWithoutUpdatingStatus = await Task.WhenAll(tasks.Select(t => t.task));
            return allFinishedTasksWithoutUpdatingStatus.ToList();
        }

        var doneActivityCount = 0;
        context.SetCustomStatus(activityStatuses.Select(s => s.custumStatus));

        while (doneActivityCount < tasks.Count)
        {
            // Wait for one of the not done tasks to complete
            var notDoneTasks = tasks.Where(t => TaskNotDone(t.task.Status)).Select(t => t.task);

            var doneTask = await Task.WhenAny(notDoneTasks);

            // Find which one completed
            var doneTaskIndex = tasks.FindIndex(t => ReferenceEquals(t.task, doneTask));

            // Sanity check
            if (doneTaskIndex < 0 || !TaskNotDone(activityStatuses[doneTaskIndex].TaskStatus))
            {
                throw new Exception(
                    "Something went wrong, completed task not found or it was already completed");
            }

            //Here we just update the existing status.
            activityStatuses[doneTaskIndex] = UpdateStatusForFinishedTask(activityStatuses[doneTaskIndex].custumStatus, doneTask);
            doneActivityCount++;

            // Only update status when not replaying
            if (!context.IsReplaying)
            {
                context.SetCustomStatus(activityStatuses.Select(s => s.custumStatus));
            }
        }

        var failedTasks = tasks.Select(t => t.task).Where(t => t.Exception != null).ToList();
        if (failedTasks.Count > 0)
        {
            throw new AggregateException(
                "One or more operations failed", failedTasks.Select(t => t.Exception));
        }
        var allFinishedTasks = await Task.WhenAll(tasks.Select(t => t.task));
        return allFinishedTasks.ToList();
    }

    private static bool TaskNotDone(TaskStatus t)
        => t is TaskStatus.WaitingForActivation or TaskStatus.Created
            or TaskStatus.Running or TaskStatus.Running or TaskStatus.WaitingForChildrenToComplete or TaskStatus.WaitingToRun;

    [Deterministic]
    private static (TaskStatus, string) GetActivityStatusFromTaskAndParam((string statusInput, Task<string>) item)
    {
        var (parameters, task) = item;
        var pendingActivity = (task.Status, $"{parameters} : ");
        var finishedActivity = (task.Status, $"{parameters} : Finished");
        var failedActivity = (task.Status, $"{parameters} : Failed");
        return task.Status switch
        {
            TaskStatus.Created => pendingActivity,
            TaskStatus.WaitingForActivation => pendingActivity,
            TaskStatus.WaitingToRun => pendingActivity,
            TaskStatus.Running => pendingActivity,
            TaskStatus.WaitingForChildrenToComplete => pendingActivity,
            TaskStatus.RanToCompletion => finishedActivity,
            TaskStatus.Canceled => failedActivity,
            TaskStatus.Faulted => failedActivity,
            _ => throw new NotImplementedException()
        };
    }

    [Deterministic]
    private static (TaskStatus, string) UpdateStatusForFinishedTask(
        string activityStatus, Task task)
    {
        if (!task.IsCompleted)
        {
            throw new Exception(
                "Should only be called on completed tasks");
        }

        return task.Status switch
        {
            TaskStatus.RanToCompletion => (task.Status, activityStatus + "Finished"),
            TaskStatus.Canceled => (task.Status, activityStatus + "Canceled"),
            TaskStatus.Faulted => (task.Status, activityStatus + "Failed"),
            _ => throw new NotImplementedException(),
        };
    }
}