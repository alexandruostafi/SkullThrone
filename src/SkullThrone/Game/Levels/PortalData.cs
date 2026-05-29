namespace SkullThrone.Game.Levels;

/// <summary>
/// Defines a bidirectional portal linking two wall tiles in the map.
/// Each endpoint has a face direction indicating where the player exits.
/// </summary>
public sealed class PortalData
{
    /// <summary>Unique identifier for this portal pair.</summary>
    public string Id { get; }

    /// <summary>First endpoint of the portal.</summary>
    public PortalEndpoint TileA { get; }

    /// <summary>Second endpoint of the portal.</summary>
    public PortalEndpoint TileB { get; }

    /// <summary>Portal color name used for rendering (e.g., "green", "purple").</summary>
    public string Color { get; }

    public PortalData(string id, PortalEndpoint tileA, PortalEndpoint tileB, string color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(color);

        Id = id;
        TileA = tileA;
        TileB = tileB;
        Color = color;
    }

    /// <summary>
    /// Given one endpoint's tile coordinates, returns the other endpoint.
    /// Returns null if the coordinates don't match either endpoint.
    /// </summary>
    public PortalEndpoint? GetDestination(int tileX, int tileY)
    {
        if (TileA.X == tileX && TileA.Y == tileY)
            return TileB;
        if (TileB.X == tileX && TileB.Y == tileY)
            return TileA;
        return null;
    }
}
