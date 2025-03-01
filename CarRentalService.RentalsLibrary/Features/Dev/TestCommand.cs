using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

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

public class TestCommandHandler(ILogger<TestCommandHandler> logger) : IRequestHandler<TestCommand, Unit>
{
    private readonly ILogger<TestCommandHandler> logger = logger;

    public async Task<Unit> Handle(TestCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Testing Command Handler!");
        return Unit.Value;
    }
}
