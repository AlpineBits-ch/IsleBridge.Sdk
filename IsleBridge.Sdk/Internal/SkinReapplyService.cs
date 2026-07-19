using IsleBridge.Integration.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IsleBridge.Integration.Internal;

/// <summary>
/// Implements the contract §5 skin-reapply flow: on a <c>join</c>, wait a few seconds (let
/// the engine finish its own skin load) then reapply the stored customizer; and on startup,
/// reapply for everyone already online (server boot case). The desired customizer comes from
/// the consumer's <see cref="ISkinStore"/>.
/// </summary>
internal sealed class SkinReapplyService(
    IEventStream events,
    IBridgeClient client,
    ISkinStore store,
    IOptions<IsleBridgeOptions> options,
    ILogger<SkinReapplyService> logger) : BackgroundService
{
    private readonly IsleBridgeOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Skin reapply service started (delay {Delay}s)", _options.SkinReapplyDelay.TotalSeconds);

        await ReapplyEveryoneOnline(stoppingToken);

        try
        {
            await foreach (var evt in events.StreamAsync(stoppingToken))
            {
                if (evt.Kind == EventKind.Join)
                    _ = ReapplyAfterDelay(evt.Steam, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // shutting down
        }
    }

    private async Task ReapplyEveryoneOnline(CancellationToken ct)
    {
        try
        {
            var roster = await client.GetPlayersAsync(ct);
            foreach (var steam in roster.Players)
                await Reapply(steam, ct);
        }
        catch (Exception ex)
        {
            // Best-effort: the bridge may not be up yet at startup. Joins will still be handled.
            logger.LogWarning(ex, "Boot skin reapply skipped (bridge not ready?)");
        }
    }

    private async Task ReapplyAfterDelay(string steam, CancellationToken ct)
    {
        try
        {
            await Task.Delay(_options.SkinReapplyDelay, ct);
            await Reapply(steam, ct);
        }
        catch (OperationCanceledException) { /* shutting down */ }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skin reapply failed for {Steam}", steam);
        }
    }

    private async Task Reapply(string steam, CancellationToken ct)
    {
        var skin = await store.GetAsync(steam, ct);
        if (skin is null) return;

        var result = await client.SetSkinAsync(steam, skin, ct);
        if (result.Ok)
            logger.LogDebug("Reapplied skin for {Steam}", steam);
        else
            logger.LogWarning("Skin reapply for {Steam} returned {Code}: {Msg}", steam, result.CodeRaw, result.Msg);
    }
}
