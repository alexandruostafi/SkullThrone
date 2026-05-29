namespace SkullThrone.Tests.Core.Physics;

using System;
using SkullThrone.Core.Physics;
using SkullThrone.Game.Entities;
using SkullThrone.Game.Levels;
using Xunit;

/// <summary>
/// Unit tests for <see cref="PortalTeleporter"/>.
/// ISTQB techniques: decision table testing, boundary value analysis, equivalence partitioning.
/// </summary>
public sealed class PortalTeleporterTests
{
    #region Helpers

    /// <summary>
    /// Creates a 10x10 map with a portal pair:
    /// Portal A at (3, 5) facing East (exit at 4, 5)
    /// Portal B at (7, 5) facing West (exit at 6, 5)
    /// </summary>
    private static MapData CreateTestMap()
    {
        const int size = 10;
        var tiles = new int[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        tiles[5 * size + 3] = PortalConstants.PortalTileId; // Portal A at (3, 5)
        tiles[5 * size + 7] = PortalConstants.PortalTileId; // Portal B at (7, 5)

        var portals = new[]
        {
            new PortalData(
                "test_portal",
                new PortalEndpoint { X = 3, Y = 5, Face = PortalFace.East },
                new PortalEndpoint { X = 7, Y = 5, Face = PortalFace.West },
                "green")
        };

        return new MapData(size, size, tiles, portals);
    }

    #endregion

    #region TryTeleport — Decision Table

    // Decision table:
    // | On Cooldown | Move hits portal tile | Portal exists | Result          |
    // |-------------|----------------------|---------------|-----------------|
    // | Yes         | —                    | —             | false, no move  |
    // | No          | No                   | —             | false, no move  |
    // | No          | Yes                  | No            | false, no move  |
    // | No          | Yes                  | Yes           | true, teleport  |

    [Fact]
    public void TryTeleport_PlayerMovesIntoPortalA_TeleportsToPortalBExit()
    {
        // Arrange
        var map = CreateTestMap();
        // Player at X=4.5, moveX=-1.0 → (int)(4.5-1.0)=(int)3.5=3 → hits portal A at tile (3,5)
        var player = new Player(4.5f, 5.5f, MathF.PI);
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        // Act
        bool result = teleporter.TryTeleport(player, -1.0f, 0f, map);

        // Assert
        Assert.True(result);
        // Portal B faces West → exit at (6, 5) center = (6.5, 5.5), facing West (π)
        Assert.Equal(6.5f, player.X, precision: 3);
        Assert.Equal(5.5f, player.Y, precision: 3);
        Assert.Equal(MathF.PI, player.Angle, precision: 3);
    }

    [Fact]
    public void TryTeleport_PlayerMovesIntoPortalB_TeleportsToPortalAExit()
    {
        // Arrange
        var map = CreateTestMap();
        // Position so (int)(px + mx) == 7 (portal B tile X)
        var player = new Player(6.5f, 5.5f, 0f); // Facing East
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        float moveX = 1.0f; // (int)(6.5 + 1.0) = (int)7.5 = 7 ✓

        // Act
        bool result = teleporter.TryTeleport(player, moveX, 0f, map);

        // Assert
        Assert.True(result);
        // Should exit at tile (4, 5) center → (4.5, 5.5), facing East (0)
        Assert.Equal(4.5f, player.X, precision: 3);
        Assert.Equal(5.5f, player.Y, precision: 3);
        Assert.Equal(0f, player.Angle, precision: 3);
    }

    [Fact]
    public void TryTeleport_NoPortalAtTarget_ReturnsFalse()
    {
        // Arrange
        var map = CreateTestMap();
        var player = new Player(5.5f, 5.5f, 0f); // Center, no portal nearby
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        float moveX = 0.1f; // Lands on tile 5 — no portal

        // Act
        bool result = teleporter.TryTeleport(player, moveX, 0f, map);

        // Assert
        Assert.False(result);
        Assert.Equal(5.5f, player.X); // Position unchanged
    }

    [Fact]
    public void TryTeleport_NoMovement_ReturnsFalse()
    {
        var map = CreateTestMap();
        var player = new Player(5.5f, 5.5f, 0f); // Center, not on a portal tile
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        bool result = teleporter.TryTeleport(player, 0f, 0f, map);

        Assert.False(result);
    }

    #endregion

    #region Cooldown — BVA

    [Fact]
    public void TryTeleport_OnCooldown_ReturnsFalseEvenWhenHittingPortal()
    {
        // Arrange
        var map = CreateTestMap();
        var player = new Player(4.5f, 5.5f, MathF.PI);
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        // First teleport succeeds
        bool first = teleporter.TryTeleport(player, -1.0f, 0f, map);
        Assert.True(first);

        // Reposition player near portal B for a return trip
        player.X = 6.5f;
        player.Y = 5.5f;

        // Only tick a small amount (well under 1.5s cooldown)
        teleporter.Update(0.5f);

        // Act
        bool second = teleporter.TryTeleport(player, 1.0f, 0f, map);

        // Assert
        Assert.False(second);
        Assert.True(teleporter.IsOnCooldown);
    }

    [Fact]
    public void TryTeleport_CooldownExpired_AllowsTeleportAgain()
    {
        // Arrange
        var map = CreateTestMap();
        var player = new Player(4.5f, 5.5f, MathF.PI);
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        // First teleport
        teleporter.TryTeleport(player, -1.0f, 0f, map);

        // Wait out the full cooldown
        teleporter.Update(PortalConstants.TeleportCooldownSeconds + 0.01f);

        // Reposition for return trip through portal B
        player.X = 6.5f;
        player.Y = 5.5f;

        // Act
        bool result = teleporter.TryTeleport(player, 1.0f, 0f, map);

        // Assert
        Assert.True(result);
        // After successful teleport, cooldown restarts
        Assert.True(teleporter.IsOnCooldown);
    }

    [Fact]
    public void Update_CooldownDecrementsOverTime()
    {
        var teleporter = new PortalTeleporter();

        Assert.False(teleporter.IsOnCooldown); // Initially no cooldown

        // Trigger a teleport to start cooldown
        var map = CreateTestMap();
        var player = new Player(4.5f, 5.5f, MathF.PI);
        teleporter.Update(0.016f);
        teleporter.TryTeleport(player, -1.0f, 0f, map);

        Assert.True(teleporter.IsOnCooldown);

        // Tick nearly the full cooldown
        teleporter.Update(PortalConstants.TeleportCooldownSeconds - 0.01f);
        Assert.True(teleporter.IsOnCooldown); // Still on cooldown (BVA: just under)

        // Tick past the boundary
        teleporter.Update(0.02f);
        Assert.False(teleporter.IsOnCooldown); // Cooldown expired (BVA: just over)
    }

    [Fact]
    public void Update_NoCooldownActive_DoesNotGoNegative()
    {
        var teleporter = new PortalTeleporter();

        // Multiple updates with no teleport should be harmless
        teleporter.Update(1.0f);
        teleporter.Update(1.0f);
        teleporter.Update(1.0f);

        Assert.False(teleporter.IsOnCooldown);
    }

    #endregion

    #region Exit Angle — All Faces (EP: each face direction)

    [Theory]
    [InlineData(PortalFace.North, -MathF.PI / 2f)]
    [InlineData(PortalFace.South, MathF.PI / 2f)]
    [InlineData(PortalFace.East, 0f)]
    [InlineData(PortalFace.West, MathF.PI)]
    public void TryTeleport_ExitAngle_MatchesFaceDirection(PortalFace destinationFace, float expectedAngle)
    {
        // Arrange: build a map with portal B facing the specified direction
        const int size = 10;
        var tiles = new int[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        // Portal A at (3, 5) facing East (exit at 4, 5)
        tiles[5 * size + 3] = PortalConstants.PortalTileId;
        // Portal B at (5, 5) facing the test direction
        tiles[5 * size + 5] = PortalConstants.PortalTileId;

        var portals = new[]
        {
            new PortalData(
                "test_portal",
                new PortalEndpoint { X = 3, Y = 5, Face = PortalFace.East },
                new PortalEndpoint { X = 5, Y = 5, Face = destinationFace },
                "green")
        };

        // Exit tile must be empty — verify it won't be a border
        MapData.GetFaceOffset(destinationFace, out int dx, out int dy);
        int exitX = 5 + dx;
        int exitY = 5 + dy;
        tiles[exitY * size + exitX] = 0; // Ensure exit is empty

        var map = new MapData(size, size, tiles, portals);
        var player = new Player(4.5f, 5.5f, MathF.PI);
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        // Act: move into portal A at tile (3, 5)
        bool result = teleporter.TryTeleport(player, -1.0f, 0f, map);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedAngle, player.Angle, precision: 3);
    }

    #endregion

    #region Y-axis Portal Detection

    [Fact]
    public void TryTeleport_PlayerMovesIntoPortalOnYAxis_Teleports()
    {
        // Arrange: portal at tile (5, 3), player moves down into it
        const int size = 10;
        var tiles = new int[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                    tiles[y * size + x] = 1;

        tiles[3 * size + 5] = PortalConstants.PortalTileId; // Portal A at (5, 3)
        tiles[7 * size + 5] = PortalConstants.PortalTileId; // Portal B at (5, 7)

        var portals = new[]
        {
            new PortalData(
                "vertical_portal",
                new PortalEndpoint { X = 5, Y = 3, Face = PortalFace.South },
                new PortalEndpoint { X = 5, Y = 7, Face = PortalFace.North },
                "purple")
        };

        var map = new MapData(size, size, tiles, portals);
        var player = new Player(5.5f, 2.5f, MathF.PI / 2f); // Above portal A, facing down
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        // moveY that lands on tile Y=3: (int)(2.5 + 1.0) = (int)3.5 = 3 ✓
        // X doesn't change, so X-axis check: (int)(5.5 + 0) = 5, not a portal
        // Y-axis check: currentTileX=(int)5.5=5, targetTileY=(int)(2.5+1.0)=3 → portal at (5,3) ✓

        // Act
        bool result = teleporter.TryTeleport(player, 0f, 1.0f, map);

        // Assert
        Assert.True(result);
        // Exit from portal B facing North: (5, 7-1+0.5) = (5.5, 6.5)
        Assert.Equal(5.5f, player.X, precision: 3);
        Assert.Equal(6.5f, player.Y, precision: 3);
        Assert.Equal(-MathF.PI / 2f, player.Angle, precision: 3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TryTeleport_PlayerAlreadyOnPortalTileWithZeroMove_TeleportsViaXAxisCheck()
    {
        // Edge case: (int)(player.X + 0) == portal tile X AND (int)player.Y == portal tile Y
        // This means a stationary player whose truncated position matches a portal tile
        // WILL trigger teleportation — this is expected behavior because the player's
        // integer tile position overlaps the portal.
        var map = CreateTestMap();
        // Player at (3.5, 5.5) → (int)3.5=3, (int)5.5=5 → tile (3,5) is portal A
        var player = new Player(3.5f, 5.5f, 0f);
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        bool result = teleporter.TryTeleport(player, 0f, 0f, map);

        // Player's truncated X lands on portal tile → teleport triggers
        Assert.True(result);
        Assert.Equal(6.5f, player.X, precision: 3);
    }

    [Fact]
    public void TryTeleport_PlayerNearPortalButTruncatedPositionDoesNotMatch_ReturnsFalse()
    {
        // Player at (4.1, 5.5) → (int)(4.1+0)=4, not portal tile 3
        var map = CreateTestMap();
        var player = new Player(4.1f, 5.5f, 0f);
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        bool result = teleporter.TryTeleport(player, 0f, 0f, map);

        Assert.False(result);
    }

    #endregion

    #region Map With No Portals

    [Fact]
    public void TryTeleport_MapWithNoPortals_ReturnsFalse()
    {
        var map = new MapData(4, 4, new int[16]);
        var player = new Player(2.5f, 2.5f, 0f);
        var teleporter = new PortalTeleporter();
        teleporter.Update(0.016f);

        bool result = teleporter.TryTeleport(player, 0.5f, 0f, map);

        Assert.False(result);
    }

    #endregion
}
