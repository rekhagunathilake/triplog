using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Triplog.Entries.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next, // the next delegate in the pipeline, which eventually calls the request handler
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName} with content: {@Request}", requestName, request);

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();
            logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds} ms with response: {@Response}", requestName, stopwatch.ElapsedMilliseconds, response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds} ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
