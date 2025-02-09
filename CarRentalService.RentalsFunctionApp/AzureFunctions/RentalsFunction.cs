using CarRentalService.CommonLibrary.Constants;
using CarRentalService.RentalsLibrary.Constants;
using CarRentalService.RentalsLibrary.Features.Dev;
using CarRentalService.RentalsLibrary.MappingProfiles;
using CarRentalService.RentalsLibrary.Models.Messaging;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CarRentalService.RentalsFunctionApp.AzureFunctions;

public class RentalsFunction(ILogger<RentalsFunction> logger, IMediator mediator)
{
    [Function(nameof(Test))]
    public async Task<IActionResult> Test([HttpTrigger(AuthorizationLevel.Function, TriggerHttpMethods.Get, TriggerHttpMethods.Post)] HttpRequestData req)
    {
        await mediator.Send(new TestCommand(2, "John"));
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }

    [Function(nameof(ProcessCreateRentalMessage))]
    public async Task ProcessCreateRentalMessage([RabbitMQTrigger(AppConsts.CreateRentalQueue, ConnectionStringSetting = AppConsts.RabbitMQConnectionString)] QueueItemBase<CreateRentalMessage> queueItem)
    {
        var command = queueItem.Message.MapToCommand();
        await mediator.Send(command);
        Console.WriteLine("Message Captured from CarRentalApi :D");
    }
}
