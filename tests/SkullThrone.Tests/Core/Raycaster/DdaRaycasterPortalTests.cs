namespace SkullThrone.Tests.Core.Raycaster;

using System;
using SkullThrone.Core.Raycaster;
using SkullThrone.Game.Levels;
using Xunit;

/// <summary>
/// Unit tests for portal detection in <see cref="DdaRaycaster"/>.
/// ISTQB techniques: equivalence partitioning, boundary value analysis.
/// </summary>
public sealed class DdaRaycasterPortalTests
{
    private readonly DdaRaycaster _raycaster = new();

    #region Helpers

    /// <summary>
    /// Creates a 10x10 bordered map with a portal tile at the specified position.
    /// </summary>
    private static (int[] tiles, int width, int height) CreateMapWithPortalAt(int portalX, int portalY)
    {
        const int size = 10;
        var tiles = new int[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        tiles[portalY * size + portalX] = DdaRaycaster.PortalTileId;
        return (tiles, size, size);
    }

    #endregion

    #region Portal Tile Detection — EP

    [Fact]
    public void CastAllRays_RayHitsPortalTile_RayHitMarkedAsPortal()
    {
        // Arrange: portal directly in front of player (facing East → +X)
        var (tiles, w, h) = CreateMapWithPortalAt(5, 5);
        float playerX = 3.5f;
        float playerY = 5.5f;
        float playerAngle = 0f; // Facing East

        // Act
        _raycaster.CastAllRays(playerX, playerY, playerAngle, tiles, w, h);

        // Assert: center column should hit the portal
        var buffer = _raycaster.HitBuffer;
        int centerColumn = DdaRaycaster.ScreenWidth / 2;
        ref readonly var hit = ref buffer[centerColumn];

        Assert.True(hit.IsPortal);
        Assert.Equal(DdaRaycaster.PortalTileId, hit.TextureId);
        Assert.Equal(5, hit.MapX);
        Assert.Equal(5, hit.MapY);
    }

    [Fact]
    public void CastAllRays_RayHitsRegularWall_RayHitNotMarkedAsPortal()
    {
        // Arrange: player facing East, hits the right border wall (tile ID 1)
        const int size = 10;
        var tiles = new int[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        float playerX = 5.5f;
        float playerY = 5.5f;
        float playerAngle = 0f; // Facing East

        // Act
        _raycaster.CastAllRays(playerX, playerY, playerAngle, tiles, size, size);

        // Assert: center column hits right border
        var buffer = _raycaster.HitBuffer;
        int centerColumn = DdaRaycaster.ScreenWidth / 2;
        ref readonly var hit = ref buffer[centerColumn];

        Assert.False(hit.IsPortal);
        Assert.Equal(1, hit.TextureId);
    }

    #endregion

    #region Portal Hit Data — BVA

    [Fact]
    public void CastAllRays_PortalAtDistance1_ReturnsCorrectPerpDistance()
    {
        // Arrange: portal 1 tile away from player
        var (tiles, w, h) = CreateMapWithPortalAt(5, 5);
        float playerX = 4.5f; // 1 tile to the left of portal
        float playerY = 5.5f;
        float playerAngle = 0f; // Facing East

        // Act
        _raycaster.CastAllRays(playerX, playerY, playerAngle, tiles, w, h);

        // Assert
        var buffer = _raycaster.HitBuffer;
        int centerColumn = DdaRaycaster.ScreenWidth / 2;
        ref readonly var hit = ref buffer[centerColumn];

        Assert.True(hit.IsPortal);
        Assert.True(hit.PerpDistance > 0f);
        Assert.True(hit.PerpDistance < 1.0f); // Less than 1 tile away
    }

    [Fact]
    public void CastAllRays_PortalAtGreaterDistance_StillDetectedAsPortal()
    {
        // Arrange: portal 4 tiles away from player
        var (tiles, w, h) = CreateMapWithPortalAt(7, 5);
        float playerX = 2.5f;
        float playerY = 5.5f;
        float playerAngle = 0f; // Facing East

        // Act
        _raycaster.CastAllRays(playerX, playerY, playerAngle, tiles, w, h);

        // Assert
        var buffer = _raycaster.HitBuffer;
        int centerColumn = DdaRaycaster.ScreenWidth / 2;
        ref readonly var hit = ref buffer[centerColumn];

        Assert.True(hit.IsPortal);
        Assert.Equal(7, hit.MapX);
        Assert.True(hit.PerpDistance > 3.0f); // Should be ~4.5 tiles away
        Assert.True(hit.PerpDistance < 6.0f);
    }

    [Fact]
    public void CastAllRays_PortalHit_HasValidWallX()
    {
        // Arrange
        var (tiles, w, h) = CreateMapWithPortalAt(5, 5);
        float playerX = 3.5f;
        float playerY = 5.5f;
        float playerAngle = 0f;

        // Act
        _raycaster.CastAllRays(playerX, playerY, playerAngle, tiles, w, h);

        // Assert: WallX should be in [0, 1) range
        var buffer = _raycaster.HitBuffer;
        int centerColumn = DdaRaycaster.ScreenWidth / 2;
        ref readonly var hit = ref buffer[centerColumn];

        Assert.True(hit.IsPortal);
        Assert.InRange(hit.WallX, 0f, 1f);
    }

    #endregion

    #region PortalTileId Constant

    [Fact]
    public void PortalTileId_EqualsExpectedValue()
    {
        Assert.Equal(999, DdaRaycaster.PortalTileId);
    }

    [Fact]
    public void PortalTileId_MatchesPortalConstants()
    {
        Assert.Equal(PortalConstants.PortalTileId, DdaRaycaster.PortalTileId);
    }

    #endregion
}
