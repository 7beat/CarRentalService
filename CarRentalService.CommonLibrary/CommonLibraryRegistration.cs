using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CarRentalService.CommonLibrary;

public static class CommonLibraryRegistration
{
    public static void RegisterCommonLibrary(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
