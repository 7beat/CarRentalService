using System.Net;
using CarRentalService.CommonLibrary.Constants;
using CarRentalService.CommonLibrary.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace CarRentalService.RentalsFunctionApp.Middlewares;
internal class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger) : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionMiddleware> logger = logger;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var triggerType = context.FunctionDefinition.InputBindings.Values
                        .FirstOrDefault(x => x.Type == "durableClient")?.Type
                        ?? context.FunctionDefinition.InputBindings.Values
                               .FirstOrDefault()?.Type;

            switch (triggerType)
            {
                case TriggerTypeConsts.RabbitMQTrigger:
                    if (IsRetryableException(ex))
                        throw;
                    await HandleRabbitMQException();
                    break;
                case TriggerTypeConsts.HttpTrigger:
                    await HandleHttpException(context, ex);
                    break;
                case TriggerTypeConsts.DurableClient or TriggerTypeConsts.OrchestrationTrigger or TriggerTypeConsts.ActivityTrigger:
                    if (IsRetryableException(ex))
                        throw;
                    await HandleDurableException(context, ex);
                    break;
                default:
                    logger.LogCritical("Unhandled trigger type.");
                    break;
            }
        }
    }

    private static bool IsRetryableException(Exception ex) =>
        ex is not NotFoundException;

    private async Task HandleRabbitMQException()
    {
        logger.LogError("Handling RabbitMQ Exception in Middleware");
    }

    private async Task HandleHttpException(FunctionContext context, Exception ex)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            logger.LogError("No HttpRequestData available.");
            return;
        }

        logger.LogError($"Exception occurred: {ex.Message}");

        HttpStatusCode statusCode = HttpStatusCode.BadRequest;
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = Enum.GetName(statusCode),
            Detail = ex.Message
        };

        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(problemDetails);

        context.GetInvocationResult().Value = response;
    }

    private async Task HandleDurableException(FunctionContext context, Exception ex)
    {
        logger.LogError("Handling Orchestration Exception in Middleware");
    }
}
