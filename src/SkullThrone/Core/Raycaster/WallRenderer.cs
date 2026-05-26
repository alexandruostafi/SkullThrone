namespace SkullThrone.Core.Raycaster;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Renders wall strips to the screen based on raycaster hit results.
/// Uses a single-pixel texture for solid-color rendering (v0.2.0).
/// </summary>
public sealed class WallRenderer : IDisposable
{
    private readonly Texture2D _pixel;
    private readonly Color[] _wallColors;

    private static readonly Color CeilingColor = new(20, 12, 12);
    private static readonly Color FloorColor = new(40, 30, 30);

    /// <summary>
    /// Initializes the wall renderer, generating procedural textures and pre-computing
    /// darkened variants for EW-facing wall sides.
    /// </summary>
    public WallRenderer(GraphicsDevice graphicsDevice)
    {
        _framebuffer = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];
        _framebufferTexture = new Texture2D(graphicsDevice, DdaRaycaster.ScreenWidth, DdaRaycaster.ScreenHeight);

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
    public void Draw(SpriteBatch spriteBatch, ReadOnlySpan<RayHit> hitBuffer, Rectangle destinationRect)
    {
        RenderToFramebuffer(hitBuffer);

        _framebufferTexture.SetData(_framebuffer);
        spriteBatch.Draw(
            _framebufferTexture,
            destinationRect,
            Color.White);
    }

    public void Dispose()
    {
        _framebufferTexture.Dispose();
    }

    private void RenderToFramebuffer(ReadOnlySpan<RayHit> hitBuffer)
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
}
