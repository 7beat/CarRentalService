using CarRentalService.CommonLibrary.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CarRentalService.RentalsFunctionApp.AzureFunctions;

public class RentalsFunction(ILogger<RentalsFunction> logger)
{
    [Function(nameof(Test))]
    public IActionResult Test([HttpTrigger(AuthorizationLevel.Function, TriggerHttpMethods.Get, TriggerHttpMethods.Post)] HttpRequestData req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
