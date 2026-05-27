namespace SkullThrone.Core.Raycaster;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Renders textured wall strips to the screen based on raycaster hit results.
/// Uses procedurally generated textures sampled per-pixel for each wall column.
/// </summary>
public sealed class WallRenderer : IDisposable
{
    private readonly Texture2D _framebufferTexture;
    private readonly Color[] _framebuffer;
    private readonly Color[][] _textureData;
    private readonly Color[][] _textureDataDark;

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

        Texture2D?[] textures;
        try
        {
            textures = ProceduralTextures.GenerateAll(graphicsDevice);

        // Extract pixel data from textures for direct sampling
        int texturePixelCount = ProceduralTextures.TextureWidth * ProceduralTextures.TextureHeight;
        _textureData = new Color[textures.Length][];
        _textureDataDark = new Color[textures.Length][];

        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] is null)
            {
                _textureData[i] = [];
                _textureDataDark[i] = [];
                continue;
            }

            var pixels = new Color[texturePixelCount];
            textures[i]!.GetData(pixels);
            _textureData[i] = pixels;

            // Pre-compute darkened variant for EW-facing sides
            var dark = new Color[texturePixelCount];
            for (int p = 0; p < texturePixelCount; p++)
            {
                Color c = pixels[p];
                dark[p] = new Color(c.R / 2, c.G / 2, c.B / 2);
            }

            _textureDataDark[i] = dark;

            // Dispose GPU textures - we only need the pixel arrays
            textures[i]!.Dispose();
        }
        }
        catch
        {
            _framebufferTexture.Dispose();
            throw;
        }
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

    /// <summary>Releases the framebuffer texture.</summary>
    public void Dispose()
    {
        _framebufferTexture.Dispose();
    }

    private void RenderToFramebuffer(ReadOnlySpan<RayHit> hitBuffer)
    {
        RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor);
    }

    /// <summary>
    /// Renders wall columns into a framebuffer array. Extracted as internal static for testability.
    /// </summary>
    internal static void RenderToFramebuffer(
        ReadOnlySpan<RayHit> hitBuffer,
        Color[] framebuffer,
        Color[][] textureData,
        Color[][] textureDataDark,
        Color ceilingColor,
        Color floorColor)
    {
        for (int column = 0; column < DdaRaycaster.ScreenWidth; column++)
        {
            ref readonly var hit = ref hitBuffer[column];

            int lineHeight = WallRenderingCalculations.CalculateLineHeight(hit.PerpDistance);
            int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
            int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

            // Ceiling
            for (int y = 0; y < drawStart; y++)
                framebuffer[y * DdaRaycaster.ScreenWidth + column] = ceilingColor;

            // Wall - texture-mapped
            if (hit.TextureId != 0 && hit.TextureId < textureData.Length && textureData[hit.TextureId].Length > 0)
            {
                Color[] texData = hit.IsVerticalSide ? textureData[hit.TextureId] : textureDataDark[hit.TextureId];
                int texX = (int)(hit.WallX * ProceduralTextures.TextureWidth) & (ProceduralTextures.TextureWidth - 1);

                float step = (float)ProceduralTextures.TextureHeight / lineHeight;
                float texPos = (drawStart - DdaRaycaster.ScreenHeight * 0.5f + lineHeight * 0.5f) * step;

                for (int y = drawStart; y < drawEnd; y++)
                {
                    int texY = (int)texPos & (ProceduralTextures.TextureHeight - 1);
                    texPos += step;
                    framebuffer[y * DdaRaycaster.ScreenWidth + column] = texData[texY * ProceduralTextures.TextureWidth + texX];
                }
            }

            // Floor
            for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
                framebuffer[y * DdaRaycaster.ScreenWidth + column] = floorColor;
        }
    }
}
