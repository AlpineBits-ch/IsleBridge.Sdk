using IsleBridge.Sdk.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IsleBridge.Sdk.Internal;

/// <summary>
/// Long-lived subscription to the results SSE stream. Every result line is handed to the
/// <see cref="CommandCorrelator"/> to complete the awaiting command. Runs for the app
/// lifetime and reconnects automatically via <see cref="SseClient"/>.
/// </summary>
internal sealed class ResultPump(
    SseClient sse,
    CommandCorrelator correlator,
    ILogger<ResultPump> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Result correlation pump started");
        try
        {
            await foreach (var result in sse.StreamAsync<Result>(ApiRoutes.ResultsStream, stoppingToken))
                correlator.Complete(result);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // shutting down
        }
    }
}
