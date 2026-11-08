using CleanCqrs.Application.Abstractions.Data;
using CleanCqrs.Application.Abstractions.Messaging;
using CleanCqrs.Application.Common;
using CleanCqrs.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanCqrs.Application.Products.CreateProduct;

/// <summary>
/// Handles <see cref="CreateProductCommand"/>.
/// </summary>
public sealed class CreateProductCommandHandler(IAppDbContext dbContext)
    : ICommandHandler<CreateProductCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // A unique index backs this check; a concurrent insert that slips past it surfaces as a 500.
        var nameTaken = await dbContext.Products.AnyAsync(p => p.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            return ProductErrors.DuplicateName(request.Name);
        }

        var product = Product.Create(request.Name, request.Price);
        dbContext.Products.Add(product);

        // Saved and committed by TransactionBehavior.
        return product.Id;
    }
}
