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

            int lineHeight = WallRenderingCalculations.CalculateLineHeight(hit.PerpDistance);
            int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
            int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);
            Color color = WallRenderingCalculations.GetWallColor(hit.TextureId, hit.IsVerticalSide, _wallColors);

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
