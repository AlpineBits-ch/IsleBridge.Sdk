using IsleBridge.Sdk.Models;

namespace IsleBridge.Sdk;

/// <summary>
/// Typed, awaitable access to every bridge verb. Each call sends a fire-and-forget command
/// and awaits its correlated result off the results stream, so callers get a normal
/// <c>await</c> with no HTTP, offsets, or id bookkeeping in sight. Action verbs return the
/// <see cref="Result"/> (inspect <see cref="Result.Code"/>); read verbs return typed data
/// and throw <see cref="BridgeCommandException"/> if the bridge reported failure. All calls
/// throw <see cref="BridgeTimeoutException"/> if no result arrives in time.
/// </summary>
public interface IBridgeClient
{
    // --- action verbs -----------------------------------------------------------------

    /// <summary>Live species swap in place. Use a <see cref="Species"/> constant or a full class path.</summary>
    Task<Result> SwapAsync(string steam, string species, double? growth = null, CancellationToken ct = default);
    Task<Result> SetStatsAsync(string steam, SetStatsArgs stats, CancellationToken ct = default);
    Task<Result> PrimeAsync(string steam, CancellationToken ct = default);
    Task<Result> UnprimeAsync(string steam, CancellationToken ct = default);
    Task<Result> TeleportAsync(string steam, double x, double y, double z, double? yaw = null, CancellationToken ct = default);
    Task<Result> HealAsync(string steam, CancellationToken ct = default);
    Task<Result> KillAsync(string steam, CancellationToken ct = default);
    Task<Result> SetGrowthAsync(string steam, double value, CancellationToken ct = default);

    /// <summary>Set a single vital. Use a <see cref="VitalName"/> constant for <paramref name="name"/>.</summary>
    Task<Result> SetVitalAsync(string steam, string name, double value, CancellationToken ct = default);
    Task<Result> SetSkinAsync(string steam, SkinCustomizer customizer, CancellationToken ct = default);
    Task<Result> SetMutationsAsync(string steam, SetMutationsArgs mutations, CancellationToken ct = default);
    Task<Result> NotifyAsync(string steam, string message, CancellationToken ct = default);

    // --- read verbs -------------------------------------------------------------------
    Task<PositionData> GetPosAsync(string steam, CancellationToken ct = default);
    Task<StatsSnapshot> GetStatsAsync(string steam, CancellationToken ct = default);
    Task<SkinCustomizer> GetSkinAsync(string steam, CancellationToken ct = default);
    Task<MutationsData> GetMutationsAsync(string steam, CancellationToken ct = default);
    Task<PlayersData> GetPlayersAsync(CancellationToken ct = default);
    Task<PingData> PingAsync(CancellationToken ct = default);

    /// <summary>
    /// Escape hatch: send any verb (including ones this SDK version doesn't wrap) and await
    /// the raw <see cref="Result"/>. Correlation and timeout handling are applied as usual.
    /// </summary>
    Task<Result> SendAsync(string verb, string? steam = null, object? args = null,
        TimeSpan? timeout = null, CancellationToken ct = default);
}
