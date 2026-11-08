using CleanCqrs.Application.Common;
using CleanCqrs.Domain.Common;
using FluentValidation;
using MediatR;

namespace CleanCqrs.Application.Behaviors;

/// <summary>
/// Runs all FluentValidation validators for the request. On failure it short-circuits the
/// pipeline and returns a failed result with a <see cref="ValidationError"/>; it never throws.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">A result type that can represent a failure.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IFailureFactory<TResponse>
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        Error[] errors = [.. results
            .SelectMany(r => r.Errors)
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))];

        return errors.Length == 0
            ? await next(cancellationToken)
            : TResponse.Failure(new ValidationError(errors));
    }
}
