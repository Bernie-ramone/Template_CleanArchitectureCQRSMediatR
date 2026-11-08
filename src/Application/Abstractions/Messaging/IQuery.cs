using CleanCqrs.Application.Common;
using MediatR;

namespace CleanCqrs.Application.Abstractions.Messaging;

/// <summary>
/// A request that reads state without changing it and returns a <see cref="Result{TValue}"/>.
/// </summary>
/// <typeparam name="TResponse">Type of the success value.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

/// <summary>
/// Handles a <typeparamref name="TQuery"/>.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">Type of the success value.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
