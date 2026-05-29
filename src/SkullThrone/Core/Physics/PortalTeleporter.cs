namespace SkullThrone.Core.Physics;

using System;
using SkullThrone.Game.Entities;
using SkullThrone.Game.Levels;

/// <summary>
/// Handles portal teleportation logic including cooldown management.
/// Checks if the player is attempting to move into a portal wall tile
/// and teleports them to the linked destination.
/// </summary>
public sealed class PortalTeleporter
{
    private float _cooldownRemaining;

    /// <summary>
    /// Whether the teleporter is currently in cooldown (player recently teleported).
    /// </summary>
    public bool IsOnCooldown => _cooldownRemaining > 0f;

    /// <summary>
    /// Ticks the cooldown timer. Must be called every frame unconditionally.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= deltaTime;
    }

    /// <summary>
    /// Attempts to teleport the player if they are moving into a portal tile.
    /// Should be called after movement intent is calculated but before final position is applied.
    /// Call <see cref="Update"/> before this method each frame.
    /// </summary>
    /// <param name="player">The player entity.</param>
    /// <param name="moveX">Intended X movement delta.</param>
    /// <param name="moveY">Intended Y movement delta.</param>
    /// <param name="map">The current map data.</param>
    /// <returns>True if teleportation occurred, false otherwise.</returns>
    public bool TryTeleport(Player player, float moveX, float moveY, MapData map)
    {
        if (_cooldownRemaining > 0f)
            return false;

        // Check X-axis portal collision (matching the per-axis collision approach)
        int targetTileX = (int)(player.X + moveX);
        int currentTileY = (int)player.Y;

        if (map.IsPortalTile(targetTileX, currentTileY))
        {
            if (TeleportTo(player, targetTileX, currentTileY, map))
                return true;
        }

        // Check Y-axis portal collision
        int currentTileX = (int)player.X;
        int targetTileY = (int)(player.Y + moveY);

        if (map.IsPortalTile(currentTileX, targetTileY))
        {
            if (TeleportTo(player, currentTileX, targetTileY, map))
                return true;
        }

        return false;
    }

    private bool TeleportTo(Player player, int portalTileX, int portalTileY, MapData map)
    {
        var portal = map.GetPortalAt(portalTileX, portalTileY);
        if (portal is null)
            return false;

        var destination = portal.GetDestination(portalTileX, portalTileY);
        if (destination is null)
            return false;

        var dest = destination.Value;

        // Calculate exit position: center of the tile in the face direction from destination
        MapData.GetFaceOffset(dest.Face, out int dx, out int dy);
        float exitX = dest.X + dx + 0.5f;
        float exitY = dest.Y + dy + 0.5f;

        // Calculate exit angle: facing the face direction
        float exitAngle = GetAngleFromFace(dest.Face);

        // Apply teleportation
        player.X = exitX;
        player.Y = exitY;
        player.Angle = exitAngle;

        // Start cooldown
        _cooldownRemaining = PortalConstants.TeleportCooldownSeconds;

        return true;
    }

    private static float GetAngleFromFace(PortalFace face)
    {
        return face switch
        {
            PortalFace.North => -MathF.PI / 2f,  // Facing up (negative Y)
            PortalFace.South => MathF.PI / 2f,   // Facing down (positive Y)
            PortalFace.East => 0f,                // Facing right (positive X)
            PortalFace.West => MathF.PI,          // Facing left (negative X)
            _ => 0f
        };
    }
}
