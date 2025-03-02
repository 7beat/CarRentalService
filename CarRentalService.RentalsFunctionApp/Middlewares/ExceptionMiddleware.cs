using System.Net;
using CarRentalService.CommonLibrary.Constants;
using CarRentalService.CommonLibrary.Exceptions;
using FluentValidation;
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
                        .FirstOrDefault(x => x.Type == TriggerTypeConsts.DurableClient)?.Type
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

        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        ProblemDetails problemDetails = new();
        logger.LogError($"Exception occured: {ex.Message}");

        switch (ex)
        {
            case BadRequestException badRequest:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails = new()
                {
                    Title = badRequest.Message,
                    Status = (int)statusCode,
                    Detail = badRequest.InnerException?.Message,
                    Type = nameof(BadRequestException)
                };
                break;
            case NotFoundException notFound:
                statusCode = HttpStatusCode.NotFound;
                problemDetails = new()
                {
                    Title = notFound.Message,
                    Status = (int)statusCode,
                    Detail = notFound.InnerException?.Message,
                    Type = nameof(NotFoundException)
                };
                break;
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                var errorDetails = string.Join(Environment.NewLine, validationException.Errors.Select(error => error.ErrorMessage));
                problemDetails = new()
                {
                    Title = validationException.Message,
                    Status = (int)statusCode,
                    Detail = errorDetails,
                    Type = nameof(ValidationException)
                };
                break;
            default:
                problemDetails = new()
                {
                    Title = "An unexpected error occurred.",
                    Status = (int)statusCode,
                    Detail = ex.Message,
                    Type = nameof(Exception)
                };
                break;
        }

        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(problemDetails);

        context.GetInvocationResult().Value = response;
    }

    private async Task HandleDurableException(FunctionContext context, Exception ex)
    {
        logger.LogError("Handling Orchestration Exception in Middleware");
    }
}
