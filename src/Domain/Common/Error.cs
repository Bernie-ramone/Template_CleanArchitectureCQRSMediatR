namespace CleanCqrs.Domain.Common;

/// <summary>
/// Describes an expected failure. Expected failures are returned as values, never thrown.
/// </summary>
/// <param name="Code">Stable, machine-readable code, for example <c>Product.NotFound</c>.</param>
/// <param name="Description">Human-readable explanation.</param>
/// <param name="Type">Category used to map the error to a transport-specific response.</param>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>Sentinel used by successful results.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>Creates a <see cref="ErrorType.Validation"/> error.</summary>
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    /// <summary>Creates a <see cref="ErrorType.NotFound"/> error.</summary>
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    /// <summary>Creates a <see cref="ErrorType.Conflict"/> error.</summary>
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    /// <summary>Creates a <see cref="ErrorType.Failure"/> error.</summary>
    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
}
