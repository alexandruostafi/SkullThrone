namespace SkullThrone.Core.Raycaster;

/// <summary>
/// Result data from a single ray cast against the map grid.
/// </summary>
public readonly struct RayHit
{
    /// <summary>Perpendicular distance from the camera plane to the wall hit.</summary>
    public float PerpDistance { get; init; }

    /// <summary>Whether the ray hit a vertical (NS) wall side. False means horizontal (EW).</summary>
    public bool IsVerticalSide { get; init; }

    /// <summary>The wall texture ID at the hit location (from map tile data).</summary>
    public int TextureId { get; init; }

    /// <summary>Fractional X coordinate along the wall face (0.0–1.0) for texture mapping.</summary>
    public float WallX { get; init; }
}
