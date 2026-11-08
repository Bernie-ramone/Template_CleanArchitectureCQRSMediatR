namespace CleanCqrs.Domain.Products;

/// <summary>
/// A product that can be sold.
/// </summary>
public sealed class Product
{
    /// <summary>Maximum length of <see cref="Name"/>; shared by validation and persistence.</summary>
    public const int NameMaxLength = 100;

    // Private: instances are created through Create (or rehydrated by EF Core via this constructor).
    private Product(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Unique, human-readable name.</summary>
    public string Name { get; private set; }

    /// <summary>Unit price; always greater than zero.</summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <remarks>
    /// Input is validated before it reaches the domain, so these guards protect invariants
    /// against programming errors; they are not the user-facing validation.
    /// </remarks>
    /// <exception cref="ArgumentException">An invariant is violated.</exception>
    public static Product Create(string name, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, NameMaxLength, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        // Version 7 GUIDs are time-ordered, which keeps the clustered index from fragmenting.
        return new Product(Guid.CreateVersion7(), name, price);
    }
}
