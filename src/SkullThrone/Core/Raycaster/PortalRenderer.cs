namespace SkullThrone.Core.Raycaster;

using System;
using Microsoft.Xna.Framework;

/// <summary>
/// Renders portal wall columns as a circular portal shape with flat color fill
/// and white particle dots. The circle is inscribed in the wall face.
/// Uses deterministic pseudo-random particle placement for zero allocations.
/// </summary>
public sealed class PortalRenderer
{
    private static readonly Color DefaultPortalColor = new(0, 200, 80);
    private static readonly Color BorderColor = new(20, 20, 20);
    private static readonly Color BorderHighlight = new(60, 60, 60);

    private static readonly Dictionary<string, Color> PortalColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["green"] = new Color(0, 200, 80),
        ["purple"] = new Color(160, 40, 200),
        ["blue"] = new Color(40, 80, 220),
        ["red"] = new Color(220, 40, 40),
        ["orange"] = new Color(220, 140, 20),
    };

    /// <summary>Radius squared threshold for the circle (0.5 radius in normalized space).</summary>
    private const float CircleRadiusSq = 0.25f; // 0.5^2

    /// <summary>Slightly smaller radius for the inner glow ring.</summary>
    private const float InnerRingSq = 0.20f;

    private int _frameCounter;

    /// <summary>
    /// Advances the frame counter for particle animation.
    /// Call once per frame in Update.
    /// </summary>
    public void Update()
    {
        unchecked { _frameCounter++; }
    }

    /// <summary>
    /// Renders a single portal column into the framebuffer with a circular mask.
    /// </summary>
    public void RenderColumn(
        Color[] framebuffer,
        int column,
        int drawStart,
        int drawEnd,
        float wallX,
        string? portalColor)
    {
        Color baseColor = GetColor(portalColor);
        int lineHeight = drawEnd - drawStart;

        // Normalized X position on the wall face, centered: -0.5 to +0.5
        float nx = wallX - 0.5f;

        for (int y = drawStart; y < drawEnd; y++)
        {
            if (y < 0 || y >= DdaRaycaster.ScreenHeight)
                continue;

            // Normalized Y position on the wall strip, centered: -0.5 to +0.5
            float ny = ((float)(y - drawStart) / lineHeight) - 0.5f;

            // Distance squared from center
            float distSq = nx * nx + ny * ny;

            Color pixel;
            if (distSq > CircleRadiusSq)
            {
                // Outside circle: dark border with subtle edge highlight
                float edgeDist = distSq - CircleRadiusSq;
                pixel = edgeDist < 0.02f ? BorderHighlight : BorderColor;
            }
            else if (distSq > InnerRingSq)
            {
                // Ring edge: brighter version of portal color for a glowing rim
                pixel = Brighten(baseColor);
            }
            else
            {
                // Inside circle: portal color with particles
                bool isParticle = IsParticlePixel(column, y);
                pixel = isParticle ? Color.White : baseColor;
            }

            framebuffer[y * DdaRaycaster.ScreenWidth + column] = pixel;
        }
    }

    private bool IsParticlePixel(int column, int y)
    {
        // Simple hash-based particle placement, animated by frame counter
        // Sparse particles: roughly 2-3% coverage
        int hash = HashPixel(column, y, _frameCounter / 8); // Change every 8 frames
        return (hash & 0x3F) == 0; // 1 in 64 chance
    }

    private static int HashPixel(int x, int y, int frame)
    {
        // Fast integer hash for deterministic pseudo-randomness
        int h = x * 374761393 + y * 668265263 + frame * 1274126177;
        h = (h ^ (h >> 13)) * 1274126177;
        h ^= h >> 16;
        return h;
    }

    private static Color GetColor(string? colorName)
    {
        if (colorName is null)
            return DefaultPortalColor;

        return PortalColors.TryGetValue(colorName, out var color) ? color : DefaultPortalColor;
    }

    private static Color Brighten(Color c)
    {
        return new Color(
            Math.Min(c.R + 60, 255),
            Math.Min(c.G + 60, 255),
            Math.Min(c.B + 60, 255));
    }
}
