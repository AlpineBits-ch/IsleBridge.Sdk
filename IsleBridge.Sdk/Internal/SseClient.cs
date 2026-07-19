using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using IsleBridge.Integration.Models;
using Microsoft.Extensions.Logging;

namespace IsleBridge.Integration.Internal;

/// <summary>
/// Minimal Server-Sent Events client with automatic reconnect. Yields the <c>data:</c>
/// payload of each event; the typed overload deserializes and skips malformed lines.
/// Reconnecting means the caller sees a live tail with a possible gap across a drop —
/// acceptable for chat/events/stats, and results correlation tolerates it via timeouts.
/// </summary>
internal sealed class SseClient(IHttpClientFactory factory, ILogger<SseClient> logger)
{
    public async IAsyncEnumerable<T> StreamAsync<T>(
        string path, [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var payload in StreamAsync(path, ct))
        {
            T? value = default;
            try
            {
                value = JsonSerializer.Deserialize<T>(payload, BridgeJson.Options);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Discarding malformed line on {Path}: {Payload}", path, payload);
            }

            if (value is not null) yield return value;
        }
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string path, [EnumeratorCancellation] CancellationToken ct)
    {
        var attempt = 0;

        while (!ct.IsCancellationRequested)
        {
            HttpResponseMessage? response = null;
            Stream? stream = null;
            StreamReader? reader = null;
            var connected = false;

            try
            {
                var client = factory.CreateClient(ClientNames.Stream);
                response = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();
                stream = await response.Content.ReadAsStreamAsync(ct);
                reader = new StreamReader(stream, Encoding.UTF8);
                connected = true;
                attempt = 0;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                Cleanup(reader, stream, response);
                yield break;
            }
            catch (Exception ex)
            {
                Cleanup(reader, stream, response);
                var delay = Backoff(++attempt);
                logger.LogWarning(ex, "SSE {Path} connect failed; retrying in {Delay}s", path, delay.TotalSeconds);
                await SafeDelay(delay, ct);
            }

            if (!connected) continue;

            var data = new StringBuilder();
            try
            {
                while (true)
                {
                    string? line;
                    var cancelled = false;
                    var faulted = false;

                    try
                    {
                        line = await reader!.ReadLineAsync(ct);
                    }
                    catch (OperationCanceledException)
                    {
                        cancelled = true;
                        line = null;
                    }
                    catch (Exception ex)
                    {
                        faulted = true;
                        line = null;
                        logger.LogWarning(ex, "SSE {Path} read error; reconnecting", path);
                    }

                    if (cancelled) yield break;
                    if (faulted) break;         // reconnect
                    if (line is null) break;    // server closed → reconnect

                    if (line.Length == 0)
                    {
                        if (data.Length > 0)
                        {
                            var payload = data.ToString();
                            data.Clear();
                            yield return payload;
                        }
                        continue;
                    }

                    if (line[0] == ':') continue; // comment / keep-alive

                    if (line.StartsWith("data:", StringComparison.Ordinal))
                    {
                        var segment = line[5..];
                        if (segment.StartsWith(' ')) segment = segment[1..];
                        if (data.Length > 0) data.Append('\n');
                        data.Append(segment);
                    }
                }
            }
            finally
            {
                Cleanup(reader, stream, response);
            }
        }
    }

    private static void Cleanup(StreamReader? reader, Stream? stream, HttpResponseMessage? response)
    {
        reader?.Dispose();
        stream?.Dispose();
        response?.Dispose();
    }

    private static TimeSpan Backoff(int attempt) =>
        TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, Math.Min(attempt, 5))));

    private static async Task SafeDelay(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); }
        catch (OperationCanceledException) { /* shutting down */ }
    }
}
