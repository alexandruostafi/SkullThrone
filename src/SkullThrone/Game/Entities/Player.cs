namespace SkullThrone.Game.Entities;

using System;

/// <summary>
/// Holds the player's position and facing direction in the world.
/// </summary>
public sealed class Player
{
    public float X { get; set; }
    public float Y { get; set; }

    /// <summary>Facing angle in radians.</summary>
    public float Angle { get; set; }

    public Player(float x, float y, float angle)
    {
        X = x;
        Y = y;
        Angle = angle;
    }
}
