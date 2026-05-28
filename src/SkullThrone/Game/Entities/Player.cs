namespace SkullThrone.Game.Entities;

using System;

/// <summary>
/// Holds the player's position and facing direction in the world.
/// </summary>
public sealed class Player
{
    /// <summary>Maximum pitch offset in pixels (up or down from center).</summary>
    public const int MaxPitch = 80;

    private int _pitch;

    public float X { get; set; }
    public float Y { get; set; }

    /// <summary>Facing angle in radians.</summary>
    public float Angle { get; set; }

    /// <summary>
    /// Vertical look offset in pixels (Y-shearing).
    /// Positive = looking up, negative = looking down.
    /// Clamped to ±<see cref="MaxPitch"/>.
    /// </summary>
    public int Pitch
    {
        get => _pitch;
        set => _pitch = Math.Clamp(value, -MaxPitch, MaxPitch);
    }

    public Player(float x, float y, float angle)
    {
        X = x;
        Y = y;
        Angle = angle;
    }
}
