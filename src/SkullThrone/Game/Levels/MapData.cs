namespace SkullThrone.Game.Levels;

/// <summary>
/// Represents the tile grid data for a game level.
/// Tiles are stored as a flat array in row-major order (y * Width + x).
/// Values: 0 = empty space, 1+ = wall texture ID, 999 = portal wall.
/// </summary>
public sealed class MapData
{
    public int Width { get; }
    public int Height { get; }
    public int[] Tiles { get; }
    public PortalData[] Portals { get; }

    public MapData(int width, int height, int[] tiles, PortalData[]? portals = null)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be non-negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be non-negative.");
        if (tiles.Length != width * height)
            throw new ArgumentException($"Tiles array length ({tiles.Length}) must equal width * height ({width * height}).");

        Width = width;
        Height = height;
        Tiles = tiles;
        Portals = portals ?? [];

        ValidatePortals();
    }

    public int GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return 1; // Out-of-bounds treated as solid wall

        return Tiles[y * Width + x];
    }

    /// <summary>
    /// Returns true if the tile at (x, y) is a portal wall.
    /// </summary>
    public bool IsPortalTile(int x, int y)
    {
        return GetTile(x, y) == PortalConstants.PortalTileId;
    }

    /// <summary>
    /// Finds the portal definition that has an endpoint at (tileX, tileY).
    /// Returns null if no portal exists at that location.
    /// </summary>
    public PortalData? GetPortalAt(int tileX, int tileY)
    {
        for (int i = 0; i < Portals.Length; i++)
        {
            var portal = Portals[i];
            if ((portal.TileA.X == tileX && portal.TileA.Y == tileY) ||
                (portal.TileB.X == tileX && portal.TileB.Y == tileY))
            {
                return portal;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the portal color for the portal at (tileX, tileY).
    /// Returns null if no portal exists at that location.
    /// </summary>
    public string? GetPortalColor(int tileX, int tileY)
    {
        return GetPortalAt(tileX, tileY)?.Color;
    }

    /// <summary>
    /// Creates a simple test map for development purposes.
    /// Includes two sectors separated by walls with a portal connection.
    /// </summary>
    public static MapData CreateTestMap()
    {
        const int size = 16;
        var tiles = new int[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Border walls
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                {
                    tiles[y * size + x] = 1;
                }
                // Dividing wall splitting map into two sectors (left and right)
                else if (x == 8 && y >= 1 && y <= 14)
                {
                    tiles[y * size + x] = 2;
                }
            }
        }

        // Place portal tiles on the dividing wall
        tiles[7 * size + 8] = PortalConstants.PortalTileId; // Portal A at (8, 7)

        // Place destination portal on the right sector's east border (inner wall)
        // Add a small interior wall in right sector for the destination
        tiles[12 * size + 12] = PortalConstants.PortalTileId; // Portal B at (12, 12)

        var portals = new[]
        {
            new PortalData(
                "portal_1",
                new PortalEndpoint { X = 8, Y = 7, Face = PortalFace.East },
                new PortalEndpoint { X = 12, Y = 12, Face = PortalFace.West },
                "green")
        };

        return new MapData(size, size, tiles, portals);
    }

    private void ValidatePortals()
    {
        for (int i = 0; i < Portals.Length; i++)
        {
            var portal = Portals[i];
            ValidatePortalEndpoint(portal, portal.TileA, "TileA");
            ValidatePortalEndpoint(portal, portal.TileB, "TileB");
        }
    }

    private void ValidatePortalEndpoint(PortalData portal, PortalEndpoint endpoint, string endpointName)
    {
        // Ensure the portal tile is actually marked as a portal in the grid
        if (GetTile(endpoint.X, endpoint.Y) != PortalConstants.PortalTileId)
        {
            throw new InvalidOperationException(
                $"Portal '{portal.Id}' {endpointName} at ({endpoint.X}, {endpoint.Y}) " +
                $"must be tile {PortalConstants.PortalTileId} but found {GetTile(endpoint.X, endpoint.Y)}.");
        }

        // Ensure the exit tile (one step in face direction) is empty
        GetFaceOffset(endpoint.Face, out int dx, out int dy);
        int exitX = endpoint.X + dx;
        int exitY = endpoint.Y + dy;

        if (GetTile(exitX, exitY) != 0)
        {
            throw new InvalidOperationException(
                $"Portal '{portal.Id}' {endpointName} face direction '{endpoint.Face}' " +
                $"points to non-empty tile at ({exitX}, {exitY}). Exit tile must be empty.");
        }
    }

    /// <summary>
    /// Gets the X/Y offset for a given portal face direction.
    /// </summary>
    public static void GetFaceOffset(PortalFace face, out int dx, out int dy)
    {
        (dx, dy) = face switch
        {
            PortalFace.North => (0, -1),
            PortalFace.South => (0, 1),
            PortalFace.East => (1, 0),
            PortalFace.West => (-1, 0),
            _ => (0, 0)
        };
    }
}
