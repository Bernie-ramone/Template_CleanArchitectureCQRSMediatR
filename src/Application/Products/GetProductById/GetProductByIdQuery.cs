using CleanCqrs.Application.Abstractions.Messaging;

namespace CleanCqrs.Application.Products.GetProductById;

/// <summary>
/// Gets a single product by its identifier.
/// </summary>
/// <param name="Id">The product identifier.</param>
public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;

/// <summary>
/// Read model returned by product queries.
/// </summary>
/// <param name="Id">The product identifier.</param>
/// <param name="Name">The product name.</param>
/// <param name="Price">The unit price.</param>
public sealed record ProductResponse(Guid Id, string Name, decimal Price);
