namespace SkullThrone.Game.Levels;

using SkullThrone.Core.Raycaster;

/// <summary>
/// Constants related to the portal system.
/// </summary>
public static class PortalConstants
{
    /// <summary>Special tile ID that indicates a portal wall tile in the map grid.</summary>
    public const int PortalTileId = DdaRaycaster.PortalTileId;

    /// <summary>Time in seconds before a player can use another portal after teleporting.</summary>
    public const float TeleportCooldownSeconds = 1.5f;
}
