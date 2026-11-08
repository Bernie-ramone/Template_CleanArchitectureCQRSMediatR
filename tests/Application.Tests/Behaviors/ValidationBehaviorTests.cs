using CleanCqrs.Application.Behaviors;
using CleanCqrs.Application.Common;
using CleanCqrs.Application.Products.CreateProduct;
using CleanCqrs.Domain.Common;
using FluentValidation;

namespace CleanCqrs.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private readonly ValidationBehavior<CreateProductCommand, Result<Guid>> _behavior =
        new([new CreateProductCommandValidator()]);

    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ReturnsValidationFailureAndDoesNotCallHandler()
    {
        var handlerCalled = false;
        Task<Result<Guid>> Next(CancellationToken _)
        {
            handlerCalled = true;
            return Task.FromResult(Result<Guid>.Success(Guid.NewGuid()));
        }

        var result = await _behavior.Handle(new CreateProductCommand("", 0m), Next, CancellationToken.None);

        handlerCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Should().BeOfType<ValidationError>()
            .Which.Errors.Select(e => e.Code)
            .Should().BeEquivalentTo(nameof(CreateProductCommand.Name), nameof(CreateProductCommand.Price));
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_CallsHandler()
    {
        var expectedId = Guid.NewGuid();

        var result = await _behavior.Handle(
            new CreateProductCommand("Keyboard", 10m),
            _ => Task.FromResult(Result<Guid>.Success(expectedId)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
    }

    [Fact]
    public async Task Handle_WhenNoValidatorsAreRegistered_CallsHandler()
    {
        var behavior = new ValidationBehavior<CreateProductCommand, Result<Guid>>(Array.Empty<IValidator<CreateProductCommand>>());

        var result = await behavior.Handle(
            new CreateProductCommand("", 0m),
            _ => Task.FromResult(Result<Guid>.Success(Guid.NewGuid())),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
