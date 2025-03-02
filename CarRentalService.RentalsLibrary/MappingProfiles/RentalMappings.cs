using CarRentalService.RentalsLibrary.Features.Dev;
using CarRentalService.RentalsLibrary.Models.Messaging;

namespace CarRentalService.RentalsLibrary.MappingProfiles;
public static class RentalMappings
{
    public static ProcessCreateRentalCommand MapToCommand(this CreateRentalMessage message) =>
        new(message.VehicleId, message.UserId, message.DateOfCreation);
}
