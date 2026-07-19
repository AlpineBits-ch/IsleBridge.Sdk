using IsleBridge.Integration.Internal;
using IsleBridge.Integration.Models;

namespace IsleBridge.Integration;

/// <summary>Live in-game chat feed (contract §8). HTTP/SSE is hidden — just enumerate.</summary>
public interface IChatStream
{
    IAsyncEnumerable<ChatMessage> StreamAsync(CancellationToken ct = default);
}

/// <summary>Live join / leave / death feed (contract §5).</summary>
public interface IEventStream
{
    IAsyncEnumerable<GameEvent> StreamAsync(CancellationToken ct = default);
}

/// <summary>
/// Live periodic per-player stats feed (contract §7). Merely enumerating this keeps the
/// server-side flood guard fed; the stream goes quiet a few seconds after you stop reading.
/// </summary>
public interface IStatsStream
{
    IAsyncEnumerable<StatsSnapshot> StreamAsync(CancellationToken ct = default);
}

internal sealed class ChatStream(SseClient sse) : IChatStream
{
    public IAsyncEnumerable<ChatMessage> StreamAsync(CancellationToken ct = default) =>
        sse.StreamAsync<ChatMessage>(ApiRoutes.ChatStream, ct);
}

internal sealed class EventStream(SseClient sse) : IEventStream
{
    public IAsyncEnumerable<GameEvent> StreamAsync(CancellationToken ct = default) =>
        sse.StreamAsync<GameEvent>(ApiRoutes.EventsStream, ct);
}

internal sealed class StatsStream(SseClient sse) : IStatsStream
{
    public IAsyncEnumerable<StatsSnapshot> StreamAsync(CancellationToken ct = default) =>
        sse.StreamAsync<StatsSnapshot>(ApiRoutes.StatsStream, ct);
}
