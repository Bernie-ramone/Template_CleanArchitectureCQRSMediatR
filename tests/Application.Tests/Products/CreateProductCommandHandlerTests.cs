using CleanCqrs.Application.Products.CreateProduct;
using CleanCqrs.Domain.Common;
using CleanCqrs.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanCqrs.Application.Tests.Products;

public sealed class CreateProductCommandHandlerTests : IDisposable
{
    private readonly TestAppDbContext _dbContext = TestAppDbContext.Create();

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task Handle_WhenNameIsUnique_AddsProductAndReturnsItsId()
    {
        var handler = new CreateProductCommandHandler(_dbContext);

        var result = await handler.Handle(new CreateProductCommand("Keyboard", 49.90m), CancellationToken.None);
        await _dbContext.SaveChangesAsync(); // Done by TransactionBehavior in the real pipeline.

        result.IsSuccess.Should().BeTrue();
        var saved = await _dbContext.Products.SingleAsync();
        saved.Id.Should().Be(result.Value);
        saved.Name.Should().Be("Keyboard");
        saved.Price.Should().Be(49.90m);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsConflictAndAddsNothing()
    {
        _dbContext.Products.Add(Product.Create("Keyboard", 10m));
        await _dbContext.SaveChangesAsync();
        var handler = new CreateProductCommandHandler(_dbContext);

        var result = await handler.Handle(new CreateProductCommand("Keyboard", 20m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be(ProductErrors.DuplicateName("Keyboard").Code);
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }
}
