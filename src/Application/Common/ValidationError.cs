using CleanCqrs.Domain.Common;

namespace CleanCqrs.Application.Common;

/// <summary>
/// A validation failure that aggregates one error per invalid field.
/// Each inner <see cref="Error.Code"/> holds the property name.
/// </summary>
/// <param name="Errors">The individual field errors.</param>
public sealed record ValidationError(IReadOnlyList<Error> Errors)
    : Error("Validation.General", "One or more validation errors occurred.", ErrorType.Validation);
