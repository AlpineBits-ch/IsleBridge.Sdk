namespace IsleBridge.Sdk.Models;

/// <summary><c>getmutations</c> payload (contract §6).</summary>
public sealed class MutationsData
{
    /// <summary>Populated fields among the 16 slots, keyed by engine slot name.</summary>
    public Dictionary<string, string>? Slots { get; init; }

    public int ElderStacks { get; init; }
    public List<string>? Unlocks { get; init; }
}

/// <summary>
/// Arguments for <c>setmutations</c> (contract §6, async). Only set what you need; the
/// <c>slotN</c> fields map to the active <c>MutationSlot1-4</c>, the rest are inherited /
/// lineage state used to restore a dino.
/// </summary>
public sealed class SetMutationsArgs
{
    public string? Slot1 { get; init; }
    public string? Slot2 { get; init; }
    public string? Slot3 { get; init; }
    public string? Slot4 { get; init; }

    public string? Parent1 { get; init; }
    public string? Parent2 { get; init; }
    public string? Parent3 { get; init; }
    public string? Parent4 { get; init; }

    public string? ElderA1 { get; init; }
    public string? ElderA2 { get; init; }
    public string? ElderA3 { get; init; }
    public string? ElderA4 { get; init; }

    public string? ElderB1 { get; init; }
    public string? ElderB2 { get; init; }
    public string? ElderB3 { get; init; }
    public string? ElderB4 { get; init; }

    /// <summary>Lineage-tier gate deciding Life-1/2/3 effective values; without it restored slots read as Life 1.</summary>
    public int? ElderStacks { get; init; }

    /// <summary>Quest-mutation unlock names, written first so quest mutations may equip.</summary>
    public List<string>? Unlocks { get; init; }
}
