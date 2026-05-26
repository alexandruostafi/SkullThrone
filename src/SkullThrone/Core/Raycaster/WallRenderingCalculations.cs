namespace SkullThrone.Core.Raycaster;

using Microsoft.Xna.Framework;

/// <summary>
/// Pure calculation logic for wall rendering, extracted for testability.
/// No MonoGame runtime dependencies (GraphicsDevice, SpriteBatch).
/// </summary>
public static class WallRenderingCalculations
{
    /// <summary>
    /// Calculates the wall strip height in pixels for a given perpendicular distance.
    /// </summary>
    public static int CalculateLineHeight(float perpDistance)
    {
        if (!(perpDistance > 0f))
            return DdaRaycaster.ScreenHeight;

        return (int)(DdaRaycaster.ScreenHeight / perpDistance);
    }

    /// <summary>
    /// Calculates the clamped draw start Y coordinate for a wall strip.
    /// </summary>
    public static int CalculateDrawStart(int lineHeight)
    {
        int drawStart = -lineHeight / 2 + DdaRaycaster.ScreenHeight / 2;
        return drawStart < 0 ? 0 : drawStart;
    }

    /// <summary>
    /// Calculates the clamped draw end Y coordinate for a wall strip.
    /// </summary>
    public static int CalculateDrawEnd(int lineHeight)
    {
        int drawEnd = lineHeight / 2 + DdaRaycaster.ScreenHeight / 2;
        return drawEnd >= DdaRaycaster.ScreenHeight ? DdaRaycaster.ScreenHeight - 1 : drawEnd;
    }

    /// <summary>
    /// Selects the wall color based on texture ID and side orientation.
    /// Horizontal (EW) sides are darkened by 0.6 for depth perception.
    /// </summary>
    public static Color GetWallColor(int textureId, bool isVerticalSide, ReadOnlySpan<Color> wallColors)
    {
        int colorIndex = (textureId > 0 && textureId < wallColors.Length) ? textureId : 1;
        Color color = wallColors[colorIndex];

        if (!isVerticalSide)
        {
            color = new Color(
                (byte)(color.R * 0.6f),
                (byte)(color.G * 0.6f),
                (byte)(color.B * 0.6f));
        }

        return color;
    }
}
