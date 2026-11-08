using CleanCqrs.Application.Common;
using MediatR;

namespace CleanCqrs.Application.Abstractions.Messaging;

/// <summary>
/// Non-generic marker shared by all commands, so pipeline behaviors can be constrained to commands only.
/// </summary>
public interface IBaseCommand;

/// <summary>
/// A request that changes state and returns a <see cref="Result{TValue}"/>.
/// </summary>
/// <typeparam name="TResponse">Type of the success value.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

/// <summary>
/// Handles a <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">Type of the success value.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
