using CarRentalService.CommonLibrary;
using Microsoft.Extensions.DependencyInjection;

namespace CarRentalService.RentalsLibrary;

public static class RentalsLibraryRegistration
{
    public static void RegisterRentalsLibrary(this IServiceCollection services)
    {
        services.RegisterCommonLibrary();
    }
}
