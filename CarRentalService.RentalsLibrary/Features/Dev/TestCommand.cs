using System.Diagnostics;
using FluentValidation;
using MediatR;

namespace CarRentalService.RentalsLibrary.Features.Dev;
public record TestCommand(int Id, string Name) : IRequest;

public class TestCommandValidator : AbstractValidator<TestCommand>
{
    public TestCommandValidator()
    {
        RuleFor(t => t.Name)
            .NotEmpty();

        RuleFor(t => t.Id)
            .Equal(1);
    }
}

public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public async Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Logging from Test Command, Id: {request.Id}, Name: {request.Name}");

    }
}
