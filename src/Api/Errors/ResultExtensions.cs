using CleanCqrs.Application.Common;
using CleanCqrs.Domain.Common;

namespace CleanCqrs.Api.Errors;

/// <summary>
/// Maps failed <see cref="Result"/>s to RFC 9457 problem details.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Converts a failed result to a ProblemDetails (or ValidationProblemDetails) response.
    /// </summary>
    /// <exception cref="InvalidOperationException">The result is successful.</exception>
    public static IResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be converted to a problem.");
        }

        if (result.Error is ValidationError validation)
        {
            var errors = validation.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Results.ValidationProblem(errors, type: TypeFor(ErrorType.Validation));
        }

        var error = result.Error;
        return Results.Problem(
            statusCode: StatusCodeFor(error.Type),
            title: TitleFor(error.Type),
            detail: error.Description,
            type: TypeFor(error.Type),
            extensions: new Dictionary<string, object?> { ["errorCode"] = error.Code });
    }

    private static int StatusCodeFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string TitleFor(ErrorType type) => type switch
    {
        ErrorType.Validation => "Bad Request",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        _ => "Server Error",
    };

    // Links point at the RFC 9110 section defining each status code.
    private static string TypeFor(ErrorType type) => type switch
    {
        ErrorType.Validation => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
    };
}
