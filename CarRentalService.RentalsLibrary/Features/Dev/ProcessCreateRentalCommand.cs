using System.Diagnostics;
using FluentValidation;
using MediatR;

namespace CarRentalService.RentalsLibrary.Features.Dev;
public record ProcessCreateRentalCommand(Guid VehicleId, Guid UserId, DateTime DateOfCreation) : IRequest<Unit>;

public class ProcessCreateRentalCommandValidator : AbstractValidator<ProcessCreateRentalCommand>
{
    public ProcessCreateRentalCommandValidator()
    {
        RuleFor(c => c.VehicleId)
            .NotEmpty();

        RuleFor(c => c.UserId)
            .NotEmpty();

        //RuleFor(c => c.DateOfCreation)
        //    .GreaterThanOrEqualTo(DateTime.Now);
    }
}

internal class ProcessCreateRentalCommandHandler : IRequestHandler<ProcessCreateRentalCommand, Unit>
{
    public async Task<Unit> Handle(ProcessCreateRentalCommand request, CancellationToken cancellationToken)
    {
        Debug.WriteLineIf(request is not null, "Command is working!");

        return Unit.Value;
    }
}
