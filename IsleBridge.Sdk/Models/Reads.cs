namespace IsleBridge.Sdk.Models;

/// <summary><c>players</c> payload.</summary>
public sealed class PlayersData
{
    public List<string> Players { get; init; } = [];
    public int Count { get; init; }
}

/// <summary><c>ping</c> payload.</summary>
public sealed class PingData
{
    public string Version { get; init; } = "";
    public int Players { get; init; }

    /// <summary>Seconds since plugin load.</summary>
    public int Uptime { get; init; }
}

/// <summary>
/// Arguments for <c>setstats</c> (contract §6). Any subset; growth is applied first,
/// prime last.
/// </summary>
public sealed class SetStatsArgs
{
    public double? Growth { get; init; }
    public double? Health { get; init; }
    public double? Stamina { get; init; }
    public double? Hunger { get; init; }
    public double? Thirst { get; init; }
    public double? Oxygen { get; init; }
    public double? Food { get; init; }
    public double? Blood { get; init; }
    public bool? Prime { get; init; }
}
