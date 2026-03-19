using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace CleanArchitecture.VerticalSlice.Application.Pipelines;

public sealed class LoggingDecorator<TRequest, TResponse>(
    ILogger<LoggingDecorator<TRequest, TResponse>> logger,
    IHandler<TRequest, TResponse> innerHandler) : IHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest command, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "Handling request {RequestName} with data {RequestData}",
            requestName,
            JsonSerializer.Serialize(command));

        try
        {
            var response = await innerHandler.HandleAsync(command, cancellationToken);
            stopwatch.Stop();

            if (response is Result result && result.IsFailure)
            {
                logger.LogWarning(
                    "Request {RequestName} failed with error code {ErrorCode} and description {ErrorDescription} in {ElapsedMs}ms",
                    requestName,
                    result.Error.Code,
                    result.Error.Description,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation(
                    "Successfully handled request {RequestName} in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "Request {RequestName} threw an exception after {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
