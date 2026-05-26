namespace SkullThrone.Core.Raycaster;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Renders wall strips to the screen based on raycaster hit results.
/// Uses a single-pixel texture for solid-color rendering (v0.2.0).
/// </summary>
public sealed class WallRenderer
{
    private readonly Texture2D _pixel;
    private readonly Color[] _wallColors;

    public WallRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        // Wall colors indexed by texture ID (0 = unused, 1+ = wall types)
        _wallColors =
        [
            Color.Black,          // 0: no wall
            Color.DarkRed,        // 1: blood-stained stone
            Color.DarkGray,       // 2: metal
            Color.DarkGoldenrod,  // 3: brass/khorne
            Color.DarkSlateGray   // 4: dark corridor
        ];
    }

    /// <summary>
    /// Draws wall columns from the raycaster hit buffer.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, ReadOnlySpan<RayHit> hitBuffer)
    {
        for (int column = 0; column < DdaRaycaster.ScreenWidth; column++)
        {
            ref readonly var hit = ref hitBuffer[column];

            if (hit.TextureId == 0)
                continue;

            // Calculate wall strip height
            int lineHeight = (int)(DdaRaycaster.ScreenHeight / hit.PerpDistance);

            int drawStart = -lineHeight / 2 + DdaRaycaster.ScreenHeight / 2;
            if (drawStart < 0) drawStart = 0;

            int drawEnd = lineHeight / 2 + DdaRaycaster.ScreenHeight / 2;
            if (drawEnd >= DdaRaycaster.ScreenHeight) drawEnd = DdaRaycaster.ScreenHeight - 1;

            // Pick color, darken horizontal sides for depth perception
            int colorIndex = hit.TextureId < _wallColors.Length ? hit.TextureId : 1;
            Color color = _wallColors[colorIndex];

            if (!hit.IsVerticalSide)
            {
                color = new Color(
                    (byte)(color.R * 0.6f),
                    (byte)(color.G * 0.6f),
                    (byte)(color.B * 0.6f));
            }

            spriteBatch.Draw(
                _pixel,
                new Rectangle(column, drawStart, 1, drawEnd - drawStart),
                color);
        }
    }

    /// <summary>
    /// Draws floor and ceiling as solid colors.
    /// </summary>
    public void DrawFloorCeiling(SpriteBatch spriteBatch)
    {
        // Ceiling
        spriteBatch.Draw(
            _pixel,
            new Rectangle(0, 0, DdaRaycaster.ScreenWidth, DdaRaycaster.ScreenHeight / 2),
            new Color(20, 12, 12));

        // Floor
        spriteBatch.Draw(
            _pixel,
            new Rectangle(0, DdaRaycaster.ScreenHeight / 2, DdaRaycaster.ScreenWidth, DdaRaycaster.ScreenHeight / 2),
            new Color(40, 30, 30));
    }
}
