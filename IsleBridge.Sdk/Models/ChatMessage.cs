using System.Text.Json.Serialization;

namespace IsleBridge.Sdk.Models;

/// <summary>A line from <c>chat.ndjson</c> (contract §8).</summary>
public sealed class ChatMessage
{
    public long Ts { get; init; }

    /// <summary>The sender's Steam64.</summary>
    public string Steam { get; init; } = "";

    /// <summary>Raw <c>EChatMode</c> value.</summary>
    public int Mode { get; init; }

    public string? ModeName { get; init; }

    /// <summary>Best-effort display name; omitted if the plugin couldn't read it — resolve from <see cref="Steam"/>.</summary>
    public string? Name { get; init; }

    public string Text { get; init; } = "";

    /// <summary>Typed view of <see cref="Mode"/>.</summary>
    [JsonIgnore]
    public ChatMode ChatMode => (ChatMode)Mode;
}

public enum ChatMode
{
    Spatial = 0,
    Global = 1,
    Admin = 2,
    Logging = 3
}
