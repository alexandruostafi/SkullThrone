namespace SkullThrone.Game.Levels;

/// <summary>
/// Represents the tile grid data for a game level.
/// Tiles are stored as a flat array in row-major order (y * Width + x).
/// Values: 0 = empty space, 1+ = wall texture ID.
/// </summary>
public sealed class MapData
{
    public int Width { get; }
    public int Height { get; }
    public int[] Tiles { get; }

    public MapData(int width, int height, int[] tiles)
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
    }

    public int GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return 1; // Out-of-bounds treated as solid wall

        return Tiles[y * Width + x];
    }

    /// <summary>
    /// Creates a simple test map for development purposes.
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
                // Some interior walls for interest
                else if (x == 5 && y >= 2 && y <= 6)
                {
                    tiles[y * size + x] = 2;
                }
                else if (x >= 8 && x <= 12 && y == 4)
                {
                    tiles[y * size + x] = 3;
                }
                else if (x == 10 && y >= 8 && y <= 12)
                {
                    tiles[y * size + x] = 4;
                }
            }
        }

        return new MapData(size, size, tiles);
    }
}
