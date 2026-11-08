using CleanCqrs.Application.Abstractions.Data;
using CleanCqrs.Application.Abstractions.Messaging;
using CleanCqrs.Application.Common;
using CleanCqrs.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanCqrs.Application.Products.GetProductById;

/// <summary>
/// Handles <see cref="GetProductByIdQuery"/>.
/// </summary>
public sealed class GetProductByIdQueryHandler(IAppDbContext dbContext)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    /// <inheritdoc />
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Project straight to the read model: no tracking, only the needed columns.
        var product = await dbContext.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null
            ? ProductErrors.NotFound(request.Id)
            : product;
    }
}
