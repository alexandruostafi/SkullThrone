namespace SkullThrone.Game.Levels;

/// <summary>
/// One endpoint of a portal, specifying its tile position and exit direction.
/// </summary>
public readonly struct PortalEndpoint
{
    /// <summary>Tile X coordinate in the map grid.</summary>
    public int X { get; init; }

    /// <summary>Tile Y coordinate in the map grid.</summary>
    public int Y { get; init; }

    /// <summary>
    /// Direction the portal opens toward. The player exits one tile in this direction
    /// from the portal wall tile, facing this direction.
    /// </summary>
    public PortalFace Face { get; init; }
}
