using System.Text.Json.Serialization;

namespace IsleBridge.Integration.Models;

/// <summary>A line from <c>events.ndjson</c> (contract §5): join / leave / death.</summary>
public sealed class GameEvent
{
    public long Ts { get; init; }

    /// <summary>Raw kind string (<c>join</c> / <c>leave</c> / <c>death</c>).</summary>
    [JsonPropertyName("kind")]
    public string KindRaw { get; init; } = "";

    public string Steam { get; init; } = "";

    /// <summary>Present on <c>death</c> only: the dino class path.</summary>
    public string? Species { get; init; }

    /// <summary>Present on <c>death</c> only.</summary>
    public Position? Pos { get; init; }

    [JsonIgnore]
    public EventKind Kind => KindRaw switch
    {
        "join" => EventKind.Join,
        "leave" => EventKind.Leave,
        "death" => EventKind.Death,
        _ => EventKind.Unknown
    };
}

public enum EventKind
{
    Unknown,
    Join,
    Leave,
    Death
}
