using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CarRentalService.CommonLibrary;

public static class CommonLibraryRegistration
{
    public static void RegisterCommonLibrary(this IServiceCollection services, ServiceRegistrationData registrationData)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(registrationData.ExecutingAssembly);
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(registrationData.ExecutingAssembly);
    }
}

public class ValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return await next();
    }
}

public record ServiceRegistrationData(Assembly ExecutingAssembly);
