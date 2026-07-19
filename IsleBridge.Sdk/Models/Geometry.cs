namespace IsleBridge.Integration.Models;

/// <summary>Engine coordinates. Note the HUD shows Y as latitude, X as longitude (contract §6 getpos).</summary>
public sealed class Position
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public sealed class Rotation
{
    public double Pitch { get; init; }
    public double Yaw { get; init; }
    public double Roll { get; init; }
}

/// <summary><c>getpos</c> payload.</summary>
public sealed class PositionData
{
    public Position Pos { get; init; } = new();
    public Rotation? Rot { get; init; }
}
