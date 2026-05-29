namespace SkullThrone.Tests.Game.Levels;

using SkullThrone.Core.Raycaster;
using SkullThrone.Game.Levels;
using Xunit;

/// <summary>
/// Unit tests for portal-related functionality in <see cref="MapData"/>.
/// ISTQB techniques: equivalence partitioning, boundary value analysis, decision tables, error guessing.
/// </summary>
public sealed class MapDataPortalTests
{
    #region Helpers

    /// <summary>
    /// Creates a 10x10 bordered map with a portal pair at specified positions.
    /// Ensures exit tiles are empty.
    /// </summary>
    private static MapData CreateMapWithPortal(
        int axX, int axY, PortalFace faceA,
        int bxX, int bxY, PortalFace faceB,
        string color = "green")
    {
        const int size = 10;
        var tiles = new int[size * size];

        // Border walls
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        // Place portal tiles
        tiles[axY * size + axX] = PortalConstants.PortalTileId;
        tiles[bxY * size + bxX] = PortalConstants.PortalTileId;

        var portals = new[]
        {
            new PortalData(
                "test_portal",
                new PortalEndpoint { X = axX, Y = axY, Face = faceA },
                new PortalEndpoint { X = bxX, Y = bxY, Face = faceB },
                color)
        };

        return new MapData(size, size, tiles, portals);
    }

    #endregion

    #region IsPortalTile — Equivalence Partitioning

    // EP: tile == 999 → true, tile != 999 → false, out of bounds → false (returns 1)

    [Fact]
    public void IsPortalTile_PortalTileCoordinates_ReturnsTrue()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        Assert.True(map.IsPortalTile(3, 3));
        Assert.True(map.IsPortalTile(7, 7));
    }

    [Fact]
    public void IsPortalTile_EmptyTile_ReturnsFalse()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        Assert.False(map.IsPortalTile(5, 5)); // Interior empty tile
    }

    [Fact]
    public void IsPortalTile_WallTile_ReturnsFalse()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        Assert.False(map.IsPortalTile(0, 0)); // Border wall
    }

    [Fact]
    public void IsPortalTile_OutOfBounds_ReturnsFalse()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        // Out of bounds returns tile value 1 (wall), not 999
        Assert.False(map.IsPortalTile(-1, -1));
        Assert.False(map.IsPortalTile(100, 100));
    }

    #endregion

    #region GetPortalAt — Decision Table

    // | Tile coords match TileA | Tile coords match TileB | Result        |
    // |-------------------------|-------------------------|---------------|
    // | Yes                     | —                       | Portal found  |
    // | —                       | Yes                     | Portal found  |
    // | No                      | No                      | null          |

    [Fact]
    public void GetPortalAt_EndpointA_ReturnsPortal()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        var result = map.GetPortalAt(3, 3);

        Assert.NotNull(result);
        Assert.Equal("test_portal", result.Id);
    }

    [Fact]
    public void GetPortalAt_EndpointB_ReturnsPortal()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        var result = map.GetPortalAt(7, 7);

        Assert.NotNull(result);
        Assert.Equal("test_portal", result.Id);
    }

    [Fact]
    public void GetPortalAt_NoPortalAtLocation_ReturnsNull()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        Assert.Null(map.GetPortalAt(5, 5));
    }

    [Fact]
    public void GetPortalAt_EmptyPortalsList_ReturnsNull()
    {
        var map = new MapData(4, 4, new int[16]);

        Assert.Null(map.GetPortalAt(2, 2));
    }

    #endregion

    #region GetPortalColor — EP

    [Fact]
    public void GetPortalColor_PortalExists_ReturnsColor()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West, "purple");

        Assert.Equal("purple", map.GetPortalColor(3, 3));
        Assert.Equal("purple", map.GetPortalColor(7, 7));
    }

    [Fact]
    public void GetPortalColor_NoPortal_ReturnsNull()
    {
        var map = CreateMapWithPortal(3, 3, PortalFace.East, 7, 7, PortalFace.West);

        Assert.Null(map.GetPortalColor(5, 5));
    }

    #endregion

    #region Portal Validation — Error Paths

    [Fact]
    public void Constructor_PortalTileNotMarked999_ThrowsInvalidOperationException()
    {
        // Portal endpoint at (3,3) but tile is empty (0), not 999
        var tiles = new int[16];
        var portals = new[]
        {
            new PortalData(
                "bad_portal",
                new PortalEndpoint { X = 1, Y = 1, Face = PortalFace.East },
                new PortalEndpoint { X = 2, Y = 2, Face = PortalFace.West },
                "green")
        };

        Assert.Throws<InvalidOperationException>(() => new MapData(4, 4, tiles, portals));
    }

    [Fact]
    public void Constructor_PortalExitTileNotEmpty_ThrowsInvalidOperationException()
    {
        const int size = 6;
        var tiles = new int[size * size];

        // Border walls
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        // Portal A at (2, 2) facing East — exit tile is (3, 2)
        tiles[2 * size + 2] = PortalConstants.PortalTileId;
        // Block Portal A's exit with a wall — validation fails on TileA first
        tiles[2 * size + 3] = 1;

        // Portal B at (3, 3) facing North — exit tile is (3, 2), also blocked
        tiles[3 * size + 3] = PortalConstants.PortalTileId;

        var portals = new[]
        {
            new PortalData(
                "blocked_portal",
                new PortalEndpoint { X = 2, Y = 2, Face = PortalFace.East },
                new PortalEndpoint { X = 3, Y = 3, Face = PortalFace.North },
                "red")
        };

        Assert.Throws<InvalidOperationException>(() => new MapData(size, size, tiles, portals));
    }

    [Fact]
    public void Constructor_PortalExitPointsOutOfBounds_ThrowsInvalidOperationException()
    {
        const int size = 4;
        var tiles = new int[size * size];

        // Put portal on border — facing outward (exit is out of bounds → treated as wall)
        tiles[0 * size + 1] = PortalConstants.PortalTileId; // (1, 0) top border
        tiles[2 * size + 2] = PortalConstants.PortalTileId; // (2, 2) interior

        var portals = new[]
        {
            new PortalData(
                "oob_portal",
                new PortalEndpoint { X = 1, Y = 0, Face = PortalFace.North }, // Exit at (1, -1) → out of bounds
                new PortalEndpoint { X = 2, Y = 2, Face = PortalFace.South },
                "blue")
        };

        Assert.Throws<InvalidOperationException>(() => new MapData(size, size, tiles, portals));
    }

    #endregion

    #region GetFaceOffset — BVA (all four directions)

    [Theory]
    [InlineData(PortalFace.North, 0, -1)]
    [InlineData(PortalFace.South, 0, 1)]
    [InlineData(PortalFace.East, 1, 0)]
    [InlineData(PortalFace.West, -1, 0)]
    public void GetFaceOffset_Direction_ReturnsCorrectDelta(PortalFace face, int expectedDx, int expectedDy)
    {
        MapData.GetFaceOffset(face, out int dx, out int dy);

        Assert.Equal(expectedDx, dx);
        Assert.Equal(expectedDy, dy);
    }

    #endregion

    #region Multiple Portals

    [Fact]
    public void GetPortalAt_MultiplePortals_ReturnsCorrectOne()
    {
        const int size = 10;
        var tiles = new int[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        tiles[3 * size + 3] = PortalConstants.PortalTileId;
        tiles[3 * size + 7] = PortalConstants.PortalTileId;
        tiles[6 * size + 3] = PortalConstants.PortalTileId;
        tiles[6 * size + 7] = PortalConstants.PortalTileId;

        var portals = new[]
        {
            new PortalData("green_portal",
                new PortalEndpoint { X = 3, Y = 3, Face = PortalFace.East },
                new PortalEndpoint { X = 7, Y = 3, Face = PortalFace.West },
                "green"),
            new PortalData("purple_portal",
                new PortalEndpoint { X = 3, Y = 6, Face = PortalFace.East },
                new PortalEndpoint { X = 7, Y = 6, Face = PortalFace.West },
                "purple")
        };

        var map = new MapData(size, size, tiles, portals);

        Assert.Equal("green_portal", map.GetPortalAt(3, 3)!.Id);
        Assert.Equal("green_portal", map.GetPortalAt(7, 3)!.Id);
        Assert.Equal("purple_portal", map.GetPortalAt(3, 6)!.Id);
        Assert.Equal("purple_portal", map.GetPortalAt(7, 6)!.Id);
    }

    #endregion

    #region CreateTestMap — Portal Specific

    [Fact]
    public void CreateTestMap_ContainsOnePortalPair()
    {
        var map = MapData.CreateTestMap();

        Assert.Single(map.Portals);
        Assert.Equal("portal_1", map.Portals[0].Id);
    }

    [Fact]
    public void CreateTestMap_PortalTilesAreMarked999()
    {
        var map = MapData.CreateTestMap();
        var portal = map.Portals[0];

        Assert.Equal(DdaRaycaster.PortalTileId, map.GetTile(portal.TileA.X, portal.TileA.Y));
        Assert.Equal(DdaRaycaster.PortalTileId, map.GetTile(portal.TileB.X, portal.TileB.Y));
    }

    [Fact]
    public void CreateTestMap_PortalExitTilesAreEmpty()
    {
        var map = MapData.CreateTestMap();
        var portal = map.Portals[0];

        MapData.GetFaceOffset(portal.TileA.Face, out int dxA, out int dyA);
        Assert.Equal(0, map.GetTile(portal.TileA.X + dxA, portal.TileA.Y + dyA));

        MapData.GetFaceOffset(portal.TileB.Face, out int dxB, out int dyB);
        Assert.Equal(0, map.GetTile(portal.TileB.X + dxB, portal.TileB.Y + dyB));
    }

    #endregion
}
