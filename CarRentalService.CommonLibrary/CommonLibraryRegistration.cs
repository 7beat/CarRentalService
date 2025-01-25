using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CarRentalService.CommonLibrary;

public static class CommonLibraryRegistration
{
    public static void RegisterCommonLibrary(this IServiceCollection services, ServiceRegistrationData registrationData)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(registrationData.ExecutingAssembly);
        });

        services.AddValidatorsFromAssembly(registrationData.ExecutingAssembly);
    }
}

public record ServiceRegistrationData(Assembly ExecutingAssembly);
