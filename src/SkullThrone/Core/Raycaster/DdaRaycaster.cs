namespace SkullThrone.Core.Raycaster;

using System;

/// <summary>
/// Performs grid-based raycasting using the Digital Differential Analyzer (DDA) algorithm.
/// Casts one ray per screen column and writes results into a pre-allocated buffer.
/// </summary>
public sealed class DdaRaycaster
{
    public const int ScreenWidth = 320;
    public const int ScreenHeight = 200;
    public const float FieldOfView = MathF.PI / 3f; // 60 degrees
    /// <summary>Maximum ray march steps. Matches the expected maximum map dimension.</summary>
    public const int MaxRayDistance = 64;

    /// <summary>Tile ID that represents a portal wall in the map grid.</summary>
    public const int PortalTileId = 999;

    private readonly RayHit[] _hitBuffer = new RayHit[ScreenWidth];

    /// <summary>
    /// Returns the pre-allocated hit buffer. Valid after <see cref="CastAllRays"/> is called.
    /// </summary>
    public ReadOnlySpan<RayHit> HitBuffer => _hitBuffer.AsSpan();

    /// <summary>
    /// Casts rays for all screen columns against the provided map grid.
    /// </summary>
    /// <param name="playerX">Player world X position.</param>
    /// <param name="playerY">Player world Y position.</param>
    /// <param name="playerAngle">Player facing angle in radians.</param>
    /// <param name="mapTiles">2D tile grid where 0 = empty, 1+ = wall texture ID.</param>
    /// <param name="mapWidth">Width of the map grid.</param>
    /// <param name="mapHeight">Height of the map grid.</param>
    public void CastAllRays(float playerX, float playerY, float playerAngle, int[] mapTiles, int mapWidth, int mapHeight)
    {
        float dirX = MathF.Cos(playerAngle);
        float dirY = MathF.Sin(playerAngle);

        // Camera plane perpendicular to direction, scaled by half FOV tangent
        float planeScale = MathF.Tan(FieldOfView / 2f);
        float planeX = -dirY * planeScale;
        float planeY = dirX * planeScale;

        for (int column = 0; column < ScreenWidth; column++)
        {
            // Camera space: -1.0 (left) to +1.0 (right)
            float cameraX = 2f * column / ScreenWidth - 1f;

            float rayDirX = dirX + planeX * cameraX;
            float rayDirY = dirY + planeY * cameraX;

            _hitBuffer[column] = CastSingleRay(playerX, playerY, rayDirX, rayDirY, mapTiles, mapWidth, mapHeight);
        }
    }

    private static RayHit CastSingleRay(float originX, float originY, float rayDirX, float rayDirY, int[] mapTiles, int mapWidth, int mapHeight)
    {
        int mapX = (int)originX;
        int mapY = (int)originY;

        // Avoid division by zero
        float deltaDistX = rayDirX == 0f ? float.MaxValue : MathF.Abs(1f / rayDirX);
        float deltaDistY = rayDirY == 0f ? float.MaxValue : MathF.Abs(1f / rayDirY);

        int stepX;
        int stepY;
        float sideDistX;
        float sideDistY;

        if (rayDirX < 0f)
        {
            stepX = -1;
            sideDistX = (originX - mapX) * deltaDistX;
        }
        else
        {
            stepX = 1;
            sideDistX = (mapX + 1f - originX) * deltaDistX;
        }

        if (rayDirY < 0f)
        {
            stepY = -1;
            sideDistY = (originY - mapY) * deltaDistY;
        }
        else
        {
            stepY = 1;
            sideDistY = (mapY + 1f - originY) * deltaDistY;
        }

        // DDA loop
        bool isVerticalSide = false;
        int steps = 0;

        while (steps < MaxRayDistance)
        {
            if (sideDistX < sideDistY)
            {
                sideDistX += deltaDistX;
                mapX += stepX;
                isVerticalSide = true;
            }
            else
            {
                sideDistY += deltaDistY;
                mapY += stepY;
                isVerticalSide = false;
            }

            steps++;

            // Bounds check
            if (mapX < 0 || mapX >= mapWidth || mapY < 0 || mapY >= mapHeight)
                break;

            int tileValue = mapTiles[mapY * mapWidth + mapX];
            if (tileValue > 0)
            {
                // Calculate perpendicular distance (avoids fisheye)
                float perpDist = isVerticalSide
                    ? sideDistX - deltaDistX
                    : sideDistY - deltaDistY;

                // Calculate wall hit fractional position for texture mapping
                float wallX = isVerticalSide
                    ? originY + perpDist * rayDirY
                    : originX + perpDist * rayDirX;
                wallX -= MathF.Floor(wallX);

                return new RayHit
                {
                    PerpDistance = perpDist,
                    IsVerticalSide = isVerticalSide,
                    TextureId = tileValue,
                    WallX = wallX,
                    IsPortal = tileValue == PortalTileId,
                    MapX = mapX,
                    MapY = mapY
                };
            }
        }

        // No hit — max distance
        return new RayHit
        {
            PerpDistance = MaxRayDistance,
            IsVerticalSide = false,
            TextureId = 0,
            WallX = 0f
        };
    }
}
