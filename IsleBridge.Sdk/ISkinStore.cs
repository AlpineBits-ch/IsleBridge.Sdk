using IsleBridge.Sdk.Models;

namespace IsleBridge.Sdk;

/// <summary>
/// The consumer's store of desired skins per player. Skins revert on relog (the engine
/// rebuilds from the persisted <c>SkinCode</c> the plugin cannot write), so the C# side
/// owns persistence and reapplication (contract §5). Implement this over your own DB and
/// register it before enabling <see cref="IsleBridgeOptions.EnableSkinReapply"/>.
/// </summary>
public interface ISkinStore
{
    /// <summary>The customizer to (re)apply for a player, or <c>null</c> if none is stored.</summary>
    Task<SkinCustomizer?> GetAsync(string steam, CancellationToken ct = default);
}
