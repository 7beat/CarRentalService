using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;

namespace CarRentalService.CommonLibrary.Orchestration;
public class InstanceId
{
    public string Value { get; set; } = default!;

    public void Set(string value) => Value = value;

    public Task<string> Get() => Task.FromResult(Value);

    public static void Delete(TaskEntityOperation ctx) =>
        ctx.State.SetState(null);

    [Function(nameof(InstanceId))]
    public static Task Run([EntityTrigger] TaskEntityDispatcher ctx) =>
        ctx.DispatchAsync<InstanceId>();
}
