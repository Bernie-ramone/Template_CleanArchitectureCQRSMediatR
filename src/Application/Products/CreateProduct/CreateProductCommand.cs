using CleanCqrs.Application.Abstractions.Messaging;

namespace CleanCqrs.Application.Products.CreateProduct;

/// <summary>
/// Creates a product and returns its identifier.
/// </summary>
/// <param name="Name">Unique product name.</param>
/// <param name="Price">Unit price.</param>
public sealed record CreateProductCommand(string Name, decimal Price) : ICommand<Guid>;
