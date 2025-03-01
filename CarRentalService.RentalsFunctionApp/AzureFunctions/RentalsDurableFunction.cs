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
        [TimerTrigger(CronScheduleConsts.Never, RunOnStartup = false)] TimerInfo timerInfo,
        [DurableClient] DurableTaskClient durableTaskClient)
    {
        var entityId = new EntityInstanceId(OrchestrationConsts.Instance, nameof(OrchestrationTimeTrigger));
        var instanceId = await durableTaskClient.Entities.GetEntityAsync(entityId);

        if (instanceId is not null && !await HasPreviousOrchestrationEndedAsync(durableTaskClient, instanceId.State.Value))
            return;

        var newInstanceId = Guid.NewGuid().ToString();
        await durableTaskClient.Entities.SignalEntityAsync(entityId, OrchestrationConsts.SetOperation, newInstanceId);

        await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(nameof(RentalsOrchestrator), new StartOrchestrationOptions { InstanceId = newInstanceId });
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
