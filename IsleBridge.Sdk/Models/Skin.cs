using System.Text.Json.Serialization;

namespace IsleBridge.Integration.Models;

/// <summary>
/// An <c>FLinearColor</c>. Components are lowercase <c>r/g/b/a</c> per the contract;
/// nominally 0..1, values &gt;1 render as HDR glow. <c>a</c> defaults to 1.0.
/// </summary>
public sealed class SkinColor
{
    [JsonPropertyName("r")] public double R { get; init; }
    [JsonPropertyName("g")] public double G { get; init; }
    [JsonPropertyName("b")] public double B { get; init; }
    [JsonPropertyName("a")] public double A { get; init; } = 1.0;

    public SkinColor() { }

    public SkinColor(double r, double g, double b, double a = 1.0)
    {
        R = r; G = g; B = b; A = a;
    }
}

/// <summary>
/// The direct-write skin customizer (contract §6 setskin / getskin). Keys are the engine
/// field names (PascalCase), so they carry explicit attributes to bypass the camelCase
/// policy. All fields are optional — send only what you want to change. The un-readable
/// <c>SkinCode</c> is never part of this.
/// </summary>
public sealed class SkinCustomizer
{
    [JsonPropertyName("BodyColor")] public SkinColor? BodyColor { get; init; }
    [JsonPropertyName("MarkingsColor")] public SkinColor? MarkingsColor { get; init; }
    [JsonPropertyName("FlankColor")] public SkinColor? FlankColor { get; init; }
    [JsonPropertyName("UnderbellyColor")] public SkinColor? UnderbellyColor { get; init; }
    [JsonPropertyName("Detail1Color")] public SkinColor? Detail1Color { get; init; }
    [JsonPropertyName("EyesColor")] public SkinColor? EyesColor { get; init; }
    [JsonPropertyName("MaleDisplayColor")] public SkinColor? MaleDisplayColor { get; init; }

    // Added in 0.21.720.
    [JsonPropertyName("TeethColor")] public SkinColor? TeethColor { get; init; }
    [JsonPropertyName("MouthColor")] public SkinColor? MouthColor { get; init; }
    [JsonPropertyName("ClawsColor")] public SkinColor? ClawsColor { get; init; }

    [JsonPropertyName("SkinVariation")] public int? SkinVariation { get; init; }

    /// <summary>
    /// Per-species range-validated by the engine; a bad index silently drops the whole
    /// skin rebuild. You own the valid range (0 .. species-pattern-count − 1) — send a
    /// valid one or leave it null (contract §9).
    /// </summary>
    [JsonPropertyName("PatternIndex")] public int? PatternIndex { get; init; }
}
