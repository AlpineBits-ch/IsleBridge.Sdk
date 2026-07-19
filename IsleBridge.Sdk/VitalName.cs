namespace IsleBridge.Integration;

/// <summary>
/// The vital names accepted by <see cref="IBridgeClient.SetVitalAsync"/> (contract §6). The
/// method still takes a raw string; these constants exist so callers don't hand-type them.
/// </summary>
public static class VitalName
{
    public const string Health = "health";
    public const string Stamina = "stamina";
    public const string Hunger = "hunger";
    public const string Thirst = "thirst";
    public const string Oxygen = "oxygen";
    public const string Food = "food";
    public const string Blood = "blood";

    public static readonly IReadOnlyList<string> All =
        [Health, Stamina, Hunger, Thirst, Oxygen, Food, Blood];
}
