using System.Text.Json.Serialization;

namespace IsleBridge.Sdk.Models;

/// <summary>
/// One player snapshot — the shape shared by a <c>stats.ndjson</c> line and the
/// <c>getstats</c> payload (contract §6). Any section the plugin couldn't read is omitted.
/// </summary>
public sealed class StatsSnapshot
{
    public string Steam { get; init; } = "";
    public long Ts { get; init; }
    public string? Species { get; init; }
    public double Growth { get; init; }
    public Position? Pos { get; init; }

    /// <summary>Facing/orientation when the plugin reports it (may be omitted). Yaw drives proximity-voice panning.</summary>
    public Rotation? Rot { get; init; }

    public Vitals? Vitals { get; init; }
    public Nutrients? Nutrients { get; init; }

    /// <summary>Populated mutation slots keyed by engine slot name.</summary>
    public Dictionary<string, string>? Mutations { get; init; }

    public int ElderStacks { get; init; }
    public PrimeInfo? Prime { get; init; }

    [JsonPropertyName("skin")] public SkinCustomizer? Skin { get; init; }
}

public sealed class Vitals
{
    public double Hp { get; init; }
    public double HpMax { get; init; }
    public double Hunger { get; init; }
    public double HungerMax { get; init; }
    public double Thirst { get; init; }
    public double ThirstMax { get; init; }
    public double Stamina { get; init; }
    public double StaminaMax { get; init; }
    public double Food { get; init; }
    public double FoodMax { get; init; }
    public double Oxygen { get; init; }
    public double Blood { get; init; }
    public double LockedDamage { get; init; }
    public double RottenValue { get; init; }
    public double WaterLevel { get; init; }
}

public sealed class Nutrients
{
    public double Carb { get; init; }
    public double Protein { get; init; }
    public double Lipid { get; init; }
    public double Bones { get; init; }
    public double Cannibal { get; init; }
    public double Magy { get; init; }
    public double RottenFlesh { get; init; }
    public double Mushrooms { get; init; }
    public bool Malnutrition { get; init; }
}

public sealed class PrimeInfo
{
    public bool Eligible { get; init; }
    public bool[]? Conditions { get; init; }
    public int MetCount { get; init; }
}
