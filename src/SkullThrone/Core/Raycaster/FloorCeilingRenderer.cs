namespace SkullThrone.Core.Raycaster;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// Renders textured floor and ceiling pixels using per-pixel raycasting.
/// For each pixel above/below the wall strip, computes the world-space coordinate
/// and samples the appropriate texture with distance-based fog.
/// </summary>
public sealed class FloorCeilingRenderer
{
    private readonly Color[][] _floorTextures;
    private readonly Color[][] _ceilingTextures;
    private readonly int _floorTextureId;
    private readonly int _ceilingTextureId;

    /// <summary>Fog color to blend towards at distance.</summary>
    private static readonly Color FogColor = new(5, 3, 5);

    /// <summary>Distance at which fog reaches maximum intensity (0.0–1.0 blend).</summary>
    private const float FogMaxDistance = 16f;

    /// <summary>Maximum fog blend factor (keep textures somewhat visible).</summary>
    private const float FogMaxIntensity = 0.6f;

    /// <summary>
    /// Creates the floor/ceiling renderer with specified texture IDs.
    /// </summary>
    /// <param name="floorTextureId">Index into floor texture array.</param>
    /// <param name="ceilingTextureId">Index into ceiling texture array.</param>
    public FloorCeilingRenderer(int floorTextureId = 0, int ceilingTextureId = 0)
    {
        _floorTextures = ProceduralFloorCeilingTextures.GenerateFloorTextures();
        _ceilingTextures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        _floorTextureId = floorTextureId;
        _ceilingTextureId = ceilingTextureId;
    }

    /// <summary>
    /// Renders floor and ceiling pixels into the framebuffer for areas not covered by walls.
    /// Uses horizontal scanline approach: for each row, compute the distance once,
    /// then iterate across all columns.
    /// </summary>
    /// <param name="framebuffer">The pixel buffer to write into (320×200).</param>
    /// <param name="hitBuffer">Raycaster results for wall boundaries.</param>
    /// <param name="playerX">Player world X position.</param>
    /// <param name="playerY">Player world Y position.</param>
    /// <param name="playerAngle">Player facing angle in radians.</param>
    public void Render(
        Color[] framebuffer,
        ReadOnlySpan<RayHit> hitBuffer,
        float playerX,
        float playerY,
        float playerAngle,
        int pitchOffset = 0)
    {
        int screenWidth = DdaRaycaster.ScreenWidth;
        int screenHeight = DdaRaycaster.ScreenHeight;
        int horizon = screenHeight / 2 + pitchOffset;

        float dirX = MathF.Cos(playerAngle);
        float dirY = MathF.Sin(playerAngle);

        // Camera plane (same as raycaster)
        float planeScale = MathF.Tan(DdaRaycaster.FieldOfView / 2f);
        float planeX = -dirY * planeScale;
        float planeY = dirX * planeScale;

        Color[] floorTex = _floorTextures[_floorTextureId];
        Color[] ceilingTex = _ceilingTextures[_ceilingTextureId];
        int texSize = ProceduralFloorCeilingTextures.TextureSize;
        int texMask = texSize - 1; // Power-of-2 wrap

        // Render floor scanlines (below horizon)
        for (int y = horizon + 1; y < screenHeight; y++)
        {
            int rowFromHorizon = y - horizon;
            if (rowFromHorizon <= 0)
                continue;

            // Row distance: distance from camera to the floor point at this scanline
            float rowDistance = (float)(screenHeight / 2) / rowFromHorizon;

            // Calculate fog factor for this row
            float fogFactor = Math.Min(rowDistance / FogMaxDistance, 1f) * FogMaxIntensity;

            // Step vectors for this row (world-space delta per screen column)
            float floorStepX = rowDistance * (dirX + planeX - (dirX - planeX)) / screenWidth;
            float floorStepY = rowDistance * (dirY + planeY - (dirY - planeY)) / screenWidth;

            // World coordinate of the leftmost column at this row distance
            float floorX = playerX + rowDistance * (dirX - planeX);
            float floorY = playerY + rowDistance * (dirY - planeY);

            int rowOffset = y * screenWidth;

            for (int x = 0; x < screenWidth; x++)
            {
                // Check if this pixel is already a wall
                ref readonly var hit = ref hitBuffer[x];
                int lineHeight = WallRenderingCalculations.CalculateLineHeight(hit.PerpDistance);
                int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight, pitchOffset);

                // Floor pixel (below wall)
                if (y >= drawEnd)
                {
                    int texX = (int)(floorX * texSize) & texMask;
                    int texY = (int)(floorY * texSize) & texMask;
                    Color texel = floorTex[texY * texSize + texX];
                    framebuffer[rowOffset + x] = ApplyFog(texel, fogFactor);
                }

                floorX += floorStepX;
                floorY += floorStepY;
            }
        }

        // Render ceiling scanlines (above horizon)
        for (int y = 0; y < horizon; y++)
        {
            int rowFromHorizon = horizon - y;
            if (rowFromHorizon <= 0)
                continue;

            float rowDistance = (float)(screenHeight / 2) / rowFromHorizon;

            float fogFactor = Math.Min(rowDistance / FogMaxDistance, 1f) * FogMaxIntensity;

            float floorStepX = rowDistance * (dirX + planeX - (dirX - planeX)) / screenWidth;
            float floorStepY = rowDistance * (dirY + planeY - (dirY - planeY)) / screenWidth;

            float ceilX = playerX + rowDistance * (dirX - planeX);
            float ceilY = playerY + rowDistance * (dirY - planeY);

            int rowOffset = y * screenWidth;

            for (int x = 0; x < screenWidth; x++)
            {
                ref readonly var hit = ref hitBuffer[x];
                int lineHeight = WallRenderingCalculations.CalculateLineHeight(hit.PerpDistance);
                int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight, pitchOffset);

                if (y < drawStart)
                {
                    int texX = (int)(ceilX * texSize) & texMask;
                    int texY = (int)(ceilY * texSize) & texMask;
                    Color texel = ceilingTex[texY * texSize + texX];
                    framebuffer[rowOffset + x] = ApplyFog(texel, fogFactor);
                }

                ceilX += floorStepX;
                ceilY += floorStepY;
            }
        }
    }

    private static Color ApplyFog(Color texel, float fogFactor)
    {
        byte r = (byte)(texel.R + (FogColor.R - texel.R) * fogFactor);
        byte g = (byte)(texel.G + (FogColor.G - texel.G) * fogFactor);
        byte b = (byte)(texel.B + (FogColor.B - texel.B) * fogFactor);
        return new Color(r, g, b);
    }
}
