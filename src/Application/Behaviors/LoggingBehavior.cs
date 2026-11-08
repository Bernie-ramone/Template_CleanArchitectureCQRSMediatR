using System.Diagnostics;
using CleanCqrs.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCqrs.Application.Behaviors;

/// <summary>
/// Logs every request, its outcome and its duration. Outermost behavior.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        var start = Stopwatch.GetTimestamp();

        logger.Handling(requestName);
        var response = await next(cancellationToken);
        var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        if (response is Result { IsFailure: true } failed)
        {
            logger.Failed(requestName, failed.Error.Code, elapsedMs);
        }
        else
        {
            logger.Handled(requestName, elapsedMs);
        }

        return response;
    }
}

/// <summary>Source-generated, allocation-free log messages for <see cref="LoggingBehavior{TRequest, TResponse}"/>.</summary>
internal static partial class LoggingBehaviorLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    public static partial void Handling(this ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMilliseconds:0.0} ms")]
    public static partial void Handled(this ILogger logger, string requestName, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{RequestName} failed with {ErrorCode} in {ElapsedMilliseconds:0.0} ms")]
    public static partial void Failed(this ILogger logger, string requestName, string errorCode, double elapsedMilliseconds);
}
