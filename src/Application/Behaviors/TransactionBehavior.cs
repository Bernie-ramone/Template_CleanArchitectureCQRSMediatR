using CleanCqrs.Application.Abstractions.Data;
using CleanCqrs.Application.Abstractions.Messaging;
using CleanCqrs.Application.Common;
using MediatR;

namespace CleanCqrs.Application.Behaviors;

/// <summary>
/// Wraps a command in a transaction and saves its changes when the handler succeeds.
/// Constrained to <see cref="IBaseCommand"/>, so queries skip it.
/// </summary>
/// <remarks>Handlers therefore do not call <c>SaveChangesAsync</c> themselves.</remarks>
/// <typeparam name="TRequest">The command type.</typeparam>
/// <typeparam name="TResponse">The result type.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse>(IAppDbContext dbContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        // Disposing an uncommitted transaction rolls it back, which covers both failures and exceptions.
        await using var transaction = await dbContext.BeginTransactionAsync(cancellationToken);

        var response = await next(cancellationToken);
        if (response.IsFailure)
        {
            return response;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return response;
    }
}
