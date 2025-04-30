using System.Text.Json;
using CarRentalService.CommonLibrary.Constants;
using CarRentalService.CommonLibrary.Orchestration;
using CarRentalService.RentalsLibrary.Features.Dev;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace CarRentalService.RentalsFunctionApp.AzureFunctions;
public class RentalsDurableFunction(ILogger<RentalsDurableFunction> logger, IMediator mediator) : DurableFunctionBase(logger)
{
    [Function(nameof(OrchestrationTimeTrigger))]
    public async Task OrchestrationTimeTrigger(
        [TimerTrigger(CronScheduleConsts.EveryMinuteSchedule, RunOnStartup = true)] TimerInfo timerInfo,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        EntityInstanceId entityId = new("InstanceId", "RentalsDurableFunction");
        var entity = await durableTaskClient.Entities.GetEntityAsync(entityId, cancellationToken);

        var previousInstanceId = JsonSerializer.Deserialize<InstanceState>(entity?.State?.Value);
        TimeSpan orchestrationTimeout = TimeSpan.FromMinutes(10);

        if (previousInstanceId is not null)
        {
            var status = await durableTaskClient.GetInstanceAsync(previousInstanceId.Value, cancellationToken);

            if (status is not null && status.RuntimeStatus is OrchestrationRuntimeStatus.Running or OrchestrationRuntimeStatus.Pending or OrchestrationRuntimeStatus.Failed)
            {
                var createdTimeUtc = status.CreatedAt.ToUniversalTime();
                var nowUtc = DateTime.UtcNow;

                if (nowUtc - createdTimeUtc < orchestrationTimeout)
                {
                    logger.LogInformation("Previous orchestration {InstanceId} is still within timeout window. Skipping execution.", previousInstanceId.Value);
                    return;
                }
                else
                {
                    logger.LogWarning("Previous orchestration {InstanceId} exceeded timeout. Terminating...", previousInstanceId.Value);
                    await durableTaskClient.TerminateInstanceAsync(previousInstanceId.Value, "Exceeded max allowed runtime. Replacing with new instance.", cancellationToken);
                }
            }
        }

        string newInstanceId = Guid.NewGuid().ToString();
        await durableTaskClient.Entities.SignalEntityAsync(entityId, OrchestrationConsts.SetOperation, newInstanceId, cancellation: cancellationToken);

        await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
            nameof(RentalsOrchestrator),
            new StartOrchestrationOptions { InstanceId = newInstanceId }, cancellationToken);

        logger.LogInformation("Started new orchestration {InstanceId}.", newInstanceId);
    }

    /// <summary>
    /// Durable function responsible for removing expired rentals
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function(nameof(RentalsOrchestrator))]
    public async Task RentalsOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        logger.LogInformation("Rentals Orchestrator started at: {utcNow} UTC", DateTime.UtcNow);
        var result = await context.CallActivityAsync<bool>(nameof(GetRentalsAsync));
    }

    [Function(nameof(GetRentalsAsync))]
    public async Task<bool> GetRentalsAsync([ActivityTrigger] TaskActivityContext context)
    {
        //var result = await mediator.Send(new TestCommand2());
        //return result.Succeded;

        var result = await mediator.Send(new RemoveRentalCommand(Guid.NewGuid()));

        return result;
    }
}
