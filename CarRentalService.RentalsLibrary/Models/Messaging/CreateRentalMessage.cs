namespace CarRentalService.RentalsLibrary.Models.Messaging;
public class CreateRentalMessage : QueueMessage
{
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateOfCreation { get; set; }
}
