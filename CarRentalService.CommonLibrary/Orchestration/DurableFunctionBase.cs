using System.Text.Json;
using CarRentalService.CommonLibrary.Constants;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace CarRentalService.CommonLibrary.Orchestration;
public class DurableFunctionBase(ILogger<DurableFunctionBase> logger)
{
    protected async Task PurgeInstancesAsync(DurableTaskClient durableTaskClient, DateTime thresholdDateTimeUtc)
    {
        var startDateTime = DateTime.UtcNow.AddYears(-1);

        var result = await durableTaskClient.PurgeInstancesAsync(
            startDateTime,
            thresholdDateTimeUtc,
            [
                OrchestrationRuntimeStatus.Completed,
                OrchestrationRuntimeStatus.Failed,
                OrchestrationRuntimeStatus.Terminated,
                OrchestrationRuntimeStatus.Suspended,
                ]);

        logger.LogInformation("Purged {instanceCount} instances odler than {thresholdUtcTime}", result.PurgedInstanceCount, thresholdDateTimeUtc);
    }

    protected static async Task<bool> HasPreviousOrchestrationEndedAsync(DurableTaskClient durableTaskClient, string instanceId)
    {
        using var doc = JsonDocument.Parse(instanceId);
        var value = doc.RootElement.GetProperty(OrchestrationConsts.Value).GetString();

        if (!string.IsNullOrEmpty(value))
        {
            var existingInstance = await durableTaskClient.GetInstanceAsync(instanceId);

            return existingInstance is not
            {
                RuntimeStatus:
                OrchestrationRuntimeStatus.Completed or
                OrchestrationRuntimeStatus.Failed or
                OrchestrationRuntimeStatus.Terminated
            };
        }

        return false;
    }

    [Function(nameof(CleanupDurableFunction))]
    public async Task CleanupDurableFunction(
        [DurableClient] DurableTaskClient durableTaskClient,
        [TimerTrigger(OrchestrationConsts.EveryMidnightSchedule)] TimerInfo timer)
    {
        await PurgeInstancesAsync(durableTaskClient, DateTime.UtcNow.AddDays(-10));
    }
}
