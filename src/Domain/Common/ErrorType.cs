namespace CleanCqrs.Domain.Common;

/// <summary>
/// Categorizes an <see cref="Error"/> so outer layers can translate it (for example into an HTTP status code).
/// </summary>
public enum ErrorType
{
    /// <summary>Unexpected failure that the caller cannot fix.</summary>
    Failure = 0,

    /// <summary>The input is invalid.</summary>
    Validation = 1,

    /// <summary>The requested resource does not exist.</summary>
    NotFound = 2,

    /// <summary>The request conflicts with the current state (for example a duplicate).</summary>
    Conflict = 3,
}
