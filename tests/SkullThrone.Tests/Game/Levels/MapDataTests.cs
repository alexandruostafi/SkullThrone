namespace SkullThrone.Tests.Game.Levels;

using SkullThrone.Game.Levels;
using Xunit;

/// <summary>
/// Unit tests for <see cref="MapData"/>.
/// ISTQB techniques: boundary value analysis, equivalence partitioning, error guessing.
/// </summary>
public sealed class MapDataTests
{
    #region Constructor — Valid Inputs

    [Fact]
    public void Constructor_ValidTilesArray_CreatesMapData()
    {
        // Arrange & Act
        var map = new MapData(4, 4, new int[16]);

        // Assert
        Assert.Equal(4, map.Width);
        Assert.Equal(4, map.Height);
        Assert.Equal(16, map.Tiles.Length);
    }

    [Fact]
    public void Constructor_MinimalMap1x1_Succeeds()
    {
        var map = new MapData(1, 1, [0]);
        Assert.Equal(1, map.Width);
    }

    #endregion

    #region Constructor — Invalid Inputs (Error Paths)

    [Fact]
    public void Constructor_TilesLengthMismatch_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MapData(4, 4, new int[10]));
    }

    [Fact]
    public void Constructor_EmptyTilesForNonZeroDimensions_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MapData(2, 2, []));
    }

    #endregion

    #region GetTile — Equivalence Partitioning

    [Theory]
    [InlineData(0, 0, 1)]   // Top-left corner (wall in border map)
    [InlineData(1, 1, 0)]   // Interior (empty)
    [InlineData(3, 3, 1)]   // Bottom-right corner (wall)
    public void GetTile_ValidCoordinates_ReturnsExpectedValue(int x, int y, int expected)
    {
        // Arrange — 4x4 bordered map
        var tiles = new int[]
        {
            1, 1, 1, 1,
            1, 0, 0, 1,
            1, 0, 0, 1,
            1, 1, 1, 1
        };
        var map = new MapData(4, 4, tiles);

        // Act & Assert
        Assert.Equal(expected, map.GetTile(x, y));
    }

    #endregion

    #region GetTile — Boundary Value Analysis (Out-of-Bounds)

    [Theory]
    [InlineData(-1, 0)]    // Left of map
    [InlineData(0, -1)]    // Above map
    [InlineData(4, 0)]     // Right of map (width=4)
    [InlineData(0, 4)]     // Below map (height=4)
    [InlineData(-100, -100)]
    [InlineData(int.MaxValue, 0)]
    public void GetTile_OutOfBounds_ReturnsSolidWall(int x, int y)
    {
        var map = new MapData(4, 4, new int[16]);

        // Out-of-bounds is treated as solid (returns 1)
        Assert.Equal(1, map.GetTile(x, y));
    }

    #endregion

    #region GetTile — Boundary Values (Edges)

    [Theory]
    [InlineData(0, 0)]     // Min valid
    [InlineData(3, 3)]     // Max valid (width-1, height-1)
    [InlineData(3, 0)]     // Top-right
    [InlineData(0, 3)]     // Bottom-left
    public void GetTile_EdgeCoordinates_DoesNotThrow(int x, int y)
    {
        var map = new MapData(4, 4, new int[16]);
        var result = map.GetTile(x, y);
        Assert.True(result >= 0);
    }

    #endregion

    #region CreateTestMap — Smoke Test

    [Fact]
    public void CreateTestMap_Returns16x16Map()
    {
        var map = MapData.CreateTestMap();

        Assert.Equal(16, map.Width);
        Assert.Equal(16, map.Height);
        Assert.Equal(256, map.Tiles.Length);
    }

    [Fact]
    public void CreateTestMap_HasBorderWalls()
    {
        var map = MapData.CreateTestMap();

        // All border tiles should be walls
        for (int x = 0; x < 16; x++)
        {
            Assert.True(map.GetTile(x, 0) > 0, $"Top border at ({x},0) should be wall");
            Assert.True(map.GetTile(x, 15) > 0, $"Bottom border at ({x},15) should be wall");
        }
        for (int y = 0; y < 16; y++)
        {
            Assert.True(map.GetTile(0, y) > 0, $"Left border at (0,{y}) should be wall");
            Assert.True(map.GetTile(15, y) > 0, $"Right border at (15,{y}) should be wall");
        }
    }

    [Fact]
    public void CreateTestMap_InteriorHasOpenSpace()
    {
        var map = MapData.CreateTestMap();

        // Player spawn area should be open
        Assert.Equal(0, map.GetTile(1, 1));
        Assert.Equal(0, map.GetTile(2, 2));
    }

    #endregion

    #region BVA — Zero and Negative Dimensions

    [Fact]
    public void Constructor_ZeroDimensions_EmptyTiles_Succeeds()
    {
        // 0x0 map with empty array passes validation (zero is non-negative)
        var map = new MapData(0, 0, []);
        Assert.Equal(0, map.Width);
        Assert.Equal(0, map.Height);
    }

    [Fact]
    public void GetTile_OnZeroDimensionMap_AlwaysReturnsSolid()
    {
        var map = new MapData(0, 0, []);

        // Any coordinate is out-of-bounds on a 0x0 map
        Assert.Equal(1, map.GetTile(0, 0));
        Assert.Equal(1, map.GetTile(-1, -1));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    [InlineData(-1, -1)]
    [InlineData(-100, 5)]
    public void Constructor_NegativeDimensions_ThrowsArgumentOutOfRangeException(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapData(width, height, new int[1]));
    }

    #endregion

    #region EP — Various Tile Values

    [Theory]
    [InlineData(0)]    // Empty
    [InlineData(1)]    // Wall type 1
    [InlineData(255)]  // High texture ID
    [InlineData(int.MaxValue)]  // Extreme value
    public void GetTile_VariousTileValues_ReturnsStoredValue(int tileValue)
    {
        var tiles = new int[] { tileValue };
        var map = new MapData(1, 1, tiles);

        Assert.Equal(tileValue, map.GetTile(0, 0));
    }

    #endregion

    #region EP — Non-Square Maps

    [Fact]
    public void Constructor_NonSquareMap_Succeeds()
    {
        var map = new MapData(8, 2, new int[16]);

        Assert.Equal(8, map.Width);
        Assert.Equal(2, map.Height);
    }

    [Fact]
    public void GetTile_NonSquareMap_CorrectIndexing()
    {
        // 3 wide x 2 tall: row-major [y*width+x]
        var tiles = new int[]
        {
            10, 20, 30,
            40, 50, 60
        };
        var map = new MapData(3, 2, tiles);

        Assert.Equal(10, map.GetTile(0, 0));
        Assert.Equal(30, map.GetTile(2, 0));
        Assert.Equal(40, map.GetTile(0, 1));
        Assert.Equal(60, map.GetTile(2, 1));
    }

    #endregion
}
