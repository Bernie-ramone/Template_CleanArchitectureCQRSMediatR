using CleanCqrs.Domain.Common;

namespace CleanCqrs.Domain.Products;

/// <summary>
/// Expected failures related to <see cref="Product"/>.
/// </summary>
public static class ProductErrors
{
    /// <summary>No product exists with the given identifier.</summary>
    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"Product '{id}' was not found.");

    /// <summary>Another product already uses the given name.</summary>
    public static Error DuplicateName(string name) =>
        Error.Conflict("Product.DuplicateName", $"A product named '{name}' already exists.");
}
