using System.Text.Json.Serialization;

namespace IsleBridge.Sdk.Models;

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

    public static SkinColor FromHex(string? hex, double a = 1.0)
    {
        if (hex == null)
            return new SkinColor()
            {
                R = 0.0,
                G = 0.39, // Approximate value for dark green (e.g., #006400)
                B = 0.0,
                A = a
            };
        
        var skinColor = new SkinColor()
        {
            R = Convert.ToInt32(hex.Substring(0, 2), 16) / 255.0,
            G = Convert.ToInt32(hex.Substring(2, 2), 16) / 255.0,
            B = Convert.ToInt32(hex.Substring(4, 2), 16) / 255.0,
            A = a,
        };
        
        return skinColor;
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



    /// <summary>
    /// The sdk supports import from a command like this
    /// body=808080 marks=808080 flank=808080 belly=808080 detail=808080 eyes=FFFFFF male=808080 teeth=FFFFFF mouth=FF8080 claws=505050
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    public static SkinCustomizer FromProps(string props)
    {
        // body 
        var bodyHex = GetHexFromPropName("body", props);
        var bodyColor = SkinColor.FromHex(bodyHex);
        
        
        // marks 
        var marksHex = GetHexFromPropName("marks", props);
        var marksColor = SkinColor.FromHex(marksHex);
        
        // flanks
        var flanksHex = GetHexFromPropName("flank", props);
        var flanksColor = SkinColor.FromHex(flanksHex);
        
        // belly
        var bellyHex = GetHexFromPropName("belly", props);
        var bellyColor = SkinColor.FromHex(bellyHex);
        
        // details 
        var detailsHex = GetHexFromPropName("detail", props);
        var detailsColor = SkinColor.FromHex(detailsHex);
        
        // eyes
        var eyesHex = GetHexFromPropName("eyes", props);
        var eyesColor = SkinColor.FromHex(eyesHex);
        
        // male
        var maleHex = GetHexFromPropName("male", props);
        var maleColor = SkinColor.FromHex(maleHex);
        
        // teeth
        var teethHex = GetHexFromPropName("teeth", props);
        var teethColor = SkinColor.FromHex(teethHex);
        
        // mouth
        var mouthHex = GetHexFromPropName("mouth", props);
        var mouthColor = SkinColor.FromHex(mouthHex);
        
        // claws
        var clawsHex = GetHexFromPropName("claws", props);
        var clawsColor = SkinColor.FromHex(clawsHex);
        
        return new SkinCustomizer
        {
            BodyColor = bodyColor,
            MarkingsColor = marksColor,
            FlankColor = flanksColor,
            UnderbellyColor = bellyColor,
            Detail1Color = detailsColor,
            EyesColor = eyesColor,
            MaleDisplayColor = maleColor,
            TeethColor = teethColor,
            MouthColor = mouthColor,
            ClawsColor = clawsColor
        };
    }

    private static string? GetHexFromPropName(string propName, string propsStr)
    {
        var prop = propsStr.Split(' ').FirstOrDefault(p => p.StartsWith(propName + "="));
        return prop?.Split('=')[1];
    }
    
}
