using MediatR;

namespace CarRentalService.RentalsLibrary.Features.Dev;
public record class RemoveRentalCommand(Guid Id) : IRequest<bool>;

internal class RemoveRentalCommandHandler : IRequestHandler<RemoveRentalCommand, bool>
{
    public async Task<bool> Handle(RemoveRentalCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"@@ !!! Test Removing Rental with Id: {request.Id} !!! @@");
        return true;
    }
}
