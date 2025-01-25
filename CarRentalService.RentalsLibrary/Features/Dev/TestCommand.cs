using FluentValidation;
using MediatR;

namespace CarRentalService.RentalsLibrary.Features.Dev;
public record TestCommand(int Id, string Name) : IRequest<Unit>;

public class TestCommandValidator : AbstractValidator<TestCommand>
{
    public TestCommandValidator()
    {
        RuleFor(t => t.Name)
            .Equal("Test");

        RuleFor(t => t.Id)
            .Equal(1);
    }
}

public class TestCommandHandler : IRequestHandler<TestCommand, Unit>
{
    public async Task<Unit> Handle(TestCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Testing Command Handler!");
        return Unit.Value;
    }
}
