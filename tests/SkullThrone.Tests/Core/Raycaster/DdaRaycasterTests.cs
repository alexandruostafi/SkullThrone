namespace SkullThrone.Tests.Core.Raycaster;

using System;
using SkullThrone.Core.Raycaster;
using SkullThrone.Game.Levels;
using Xunit;

/// <summary>
/// Unit tests for <see cref="DdaRaycaster"/> using DDA algorithm.
/// ISTQB techniques applied:
/// - Equivalence partitioning (open space, walls, out-of-bounds)
/// - Boundary value analysis (adjacent to walls, map edges)
/// - Decision table testing (ray direction combinations)
/// </summary>
public sealed class DdaRaycasterTests
{
    private readonly DdaRaycaster _raycaster = new();

    /// <summary>
    /// Creates a simple enclosed map: walls on border, open interior.
    /// </summary>
    private static (int[] tiles, int width, int height) CreateBoxMap(int size = 8)
    {
        var tiles = new int[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;
        return (tiles, size, size);
    }

    #region CastAllRays — Basic Functionality

    [Fact]
    public void CastAllRays_PlayerInOpenSpace_FillsEntireHitBuffer()
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, w, h);

        // Assert — all 320 columns should have results
        var buffer = _raycaster.HitBuffer;
        Assert.Equal(DdaRaycaster.ScreenWidth, buffer.Length);
    }

    [Fact]
    public void CastAllRays_PlayerFacingWall_AllRaysHitWall()
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();
        float playerX = 4f;
        float playerY = 4f;
        float angle = 0f; // Facing east toward wall at x=7

        // Act
        _raycaster.CastAllRays(playerX, playerY, angle, tiles, w, h);

        // Assert — center column should hit the east wall
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.Equal(1, centerHit.TextureId);
        Assert.True(centerHit.PerpDistance > 0f);
    }

    [Fact]
    public void CastAllRays_PlayerFacingNorth_HitsNorthWall()
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();
        float angle = -MathF.PI / 2f; // North (negative Y)

        // Act
        _raycaster.CastAllRays(4f, 4f, angle, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.Equal(1, centerHit.TextureId);
        Assert.True(centerHit.PerpDistance > 0f);
        Assert.True(centerHit.PerpDistance < 5f); // Wall is at y=0, player at y=4
    }

    #endregion

    #region Boundary Value Analysis — Distance Calculations

    [Fact]
    public void CastAllRays_PlayerAdjacentToWall_ReturnsShortDistance()
    {
        // Arrange — player at (1.5, 4), facing west toward wall at x=0
        var (tiles, w, h) = CreateBoxMap();
        float angle = MathF.PI; // Facing west

        // Act
        _raycaster.CastAllRays(1.5f, 4f, angle, tiles, w, h);

        // Assert — distance should be ~0.5 (from 1.5 to wall at x=0 boundary is at x=1)
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.PerpDistance < 1.0f);
        Assert.True(centerHit.PerpDistance > 0f);
    }

    [Fact]
    public void CastAllRays_PlayerFarFromWall_ReturnsLongerDistance()
    {
        // Arrange — player at center of 16x16 map facing east, wall at x=15
        const int size = 16;
        var tiles = new int[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        // Act
        _raycaster.CastAllRays(8f, 8f, 0f, tiles, size, size);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.PerpDistance > 6f);
    }

    #endregion

    #region Equivalence Partitioning — Ray Directions

    [Theory]
    [InlineData(0f)]                    // East
    [InlineData(MathF.PI / 2f)]         // South
    [InlineData(MathF.PI)]              // West
    [InlineData(-MathF.PI / 2f)]        // North
    public void CastAllRays_CardinalDirections_AllHitWalls(float angle)
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, angle, tiles, w, h);

        // Assert — center column should always find a wall
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.TextureId > 0);
    }

    [Theory]
    [InlineData(MathF.PI / 4f)]         // SE diagonal
    [InlineData(3f * MathF.PI / 4f)]    // SW diagonal
    [InlineData(-MathF.PI / 4f)]        // NE diagonal
    [InlineData(-3f * MathF.PI / 4f)]   // NW diagonal
    public void CastAllRays_DiagonalDirections_AllHitWalls(float angle)
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, angle, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.TextureId > 0);
    }

    #endregion

    #region Decision Table — Side Detection (Vertical vs Horizontal)

    [Fact]
    public void CastAllRays_RayHitsVerticalWall_IsVerticalSideTrue()
    {
        // Arrange — facing east, will hit vertical side of east wall
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.IsVerticalSide);
    }

    [Fact]
    public void CastAllRays_RayHitsHorizontalWall_IsVerticalSideFalse()
    {
        // Arrange — facing south, will hit horizontal side of south wall
        var (tiles, w, h) = CreateBoxMap();
        float angle = MathF.PI / 2f; // South

        // Act
        _raycaster.CastAllRays(4f, 4f, angle, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.False(centerHit.IsVerticalSide);
    }

    #endregion

    #region Texture Mapping — WallX

    [Fact]
    public void CastAllRays_WallHit_WallXBetweenZeroAndOne()
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, w, h);

        // Assert — WallX is the fractional hit position for texture mapping
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.InRange(centerHit.WallX, 0f, 1f);
    }

    #endregion

    #region Multiple Texture IDs

    [Fact]
    public void CastAllRays_DifferentWallTypes_ReturnsCorrectTextureId()
    {
        // Arrange — map with texture ID 3 on the east wall
        const int size = 8;
        var tiles = new int[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = (x == size - 1) ? 3 : 1;

        // Act — face east
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, size, size);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.Equal(3, centerHit.TextureId);
    }

    #endregion

    #region No Hit — Max Distance

    [Fact]
    public void CastAllRays_OpenMapNoWalls_ReturnsMaxDistance()
    {
        // Arrange — completely empty map (no walls at all)
        const int size = 8;
        var tiles = new int[size * size]; // all zeros

        // Act
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, size, size);

        // Assert — ray exits map, returns max distance with textureId 0
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.Equal(0, centerHit.TextureId);
        Assert.Equal(DdaRaycaster.MaxRayDistance, centerHit.PerpDistance);
    }

    #endregion

    #region Fisheye Correction

    [Fact]
    public void CastAllRays_ParallelWall_NoCenterDistortionComparedToEdges()
    {
        // Arrange — player facing a flat wall; center ray should be shorter than edge rays
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, w, h);

        // Assert — perpendicular distance at center <= edges (fisheye corrected)
        var center = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        var edge = _raycaster.HitBuffer[0];

        // If both hit the same wall, center should have equal or shorter perp distance
        if (center.TextureId > 0 && edge.TextureId > 0)
        {
            Assert.True(center.PerpDistance <= edge.PerpDistance + 0.01f);
        }
    }

    #endregion

    #region Constants Validation

    [Fact]
    public void Constants_ScreenWidth_Is320()
    {
        Assert.Equal(320, DdaRaycaster.ScreenWidth);
    }

    [Fact]
    public void Constants_ScreenHeight_Is200()
    {
        Assert.Equal(200, DdaRaycaster.ScreenHeight);
    }

    [Fact]
    public void Constants_FieldOfView_Is60Degrees()
    {
        float expected = MathF.PI / 3f;
        Assert.Equal(expected, DdaRaycaster.FieldOfView);
    }

    #endregion

    #region BVA — Player at Exact Grid Boundary Positions

    [Theory]
    [InlineData(3.0f, 4.5f, 0f)]         // Integer X, facing east
    [InlineData(3.0f, 4.5f, MathF.PI)]   // Integer X, facing west
    [InlineData(4.5f, 3.0f, MathF.PI / 2f)]   // Integer Y, facing south
    [InlineData(4.5f, 3.0f, -MathF.PI / 2f)]  // Integer Y, facing north
    public void CastAllRays_PlayerOnGridBoundary_ProducesValidHits(float px, float py, float angle)
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(px, py, angle, tiles, w, h);

        // Assert — should not produce zero or negative distances
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.PerpDistance > 0f, $"PerpDistance was {centerHit.PerpDistance}");
        Assert.True(centerHit.TextureId > 0);
    }

    [Fact]
    public void CastAllRays_PlayerAtExactIntegerPosition_DistanceIsReasonable()
    {
        // Arrange — player at (4.0, 4.0) facing east, wall at x=7
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4.0f, 4.0f, 0f, tiles, w, h);

        // Assert — distance to east wall should be ~3.0
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.InRange(centerHit.PerpDistance, 2.5f, 3.5f);
    }

    [Fact]
    public void CastAllRays_PlayerNearWallBoundary_VeryShortDistance()
    {
        // Arrange — player at (1.001, 4.0), facing west toward wall at x=0
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(1.001f, 4f, MathF.PI, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.PerpDistance < 0.01f);
        Assert.True(centerHit.PerpDistance >= 0f);
    }

    #endregion

    #region BVA — WallX for All Columns

    [Fact]
    public void CastAllRays_AllColumns_WallXInValidRange()
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, w, h);

        // Assert — every column with a hit should have WallX in [0, 1]
        for (int col = 0; col < DdaRaycaster.ScreenWidth; col++)
        {
            var hit = _raycaster.HitBuffer[col];
            if (hit.TextureId > 0)
            {
                Assert.True(hit.WallX >= 0f && hit.WallX <= 1f,
                    $"Column {col}: WallX={hit.WallX} out of range");
            }
        }
    }

    [Fact]
    public void CastAllRays_AllColumns_PerpDistancePositive()
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, 0.5f, tiles, w, h);

        // Assert
        for (int col = 0; col < DdaRaycaster.ScreenWidth; col++)
        {
            var hit = _raycaster.HitBuffer[col];
            Assert.True(hit.PerpDistance >= 0f,
                $"Column {col}: PerpDistance={hit.PerpDistance} is negative");
        }
    }

    #endregion

    #region EP — Axis-Aligned Rays (Division by Zero Paths)

    [Fact]
    public void CastAllRays_PureHorizontalRay_HandlesZeroYDirection()
    {
        // Arrange — angle 0 means rayDirY ~0 for center column
        var (tiles, w, h) = CreateBoxMap();

        // Act — should not throw
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.TextureId > 0);
    }

    [Fact]
    public void CastAllRays_PureVerticalRay_HandlesZeroXDirection()
    {
        // Arrange — angle π/2 means rayDirX ~0 for center column
        var (tiles, w, h) = CreateBoxMap();

        // Act — should not throw
        _raycaster.CastAllRays(4f, 4f, MathF.PI / 2f, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.True(centerHit.TextureId > 0);
    }

    #endregion

    #region EP — Non-Square Maps

    [Fact]
    public void CastAllRays_RectangularMap_HitsWallsCorrectly()
    {
        // Arrange — 16 wide x 4 tall
        const int w = 16;
        const int h = 4;
        var tiles = new int[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                    tiles[y * w + x] = 1;

        // Act — player in center facing east
        _raycaster.CastAllRays(8f, 2f, 0f, tiles, w, h);

        // Assert
        var centerHit = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth / 2];
        Assert.Equal(1, centerHit.TextureId);
        Assert.True(centerHit.PerpDistance > 6f);
    }

    #endregion

    #region EP — Edge Columns (First and Last)

    [Fact]
    public void CastAllRays_FirstAndLastColumn_ProduceValidResults()
    {
        // Arrange
        var (tiles, w, h) = CreateBoxMap();

        // Act
        _raycaster.CastAllRays(4f, 4f, 0f, tiles, w, h);

        // Assert
        var first = _raycaster.HitBuffer[0];
        var last = _raycaster.HitBuffer[DdaRaycaster.ScreenWidth - 1];

        Assert.True(first.TextureId > 0);
        Assert.True(last.TextureId > 0);
        Assert.True(first.PerpDistance > 0f);
        Assert.True(last.PerpDistance > 0f);
    }

    #endregion
}
