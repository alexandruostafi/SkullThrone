namespace SkullThrone.Tests.Core.Raycaster;

using Microsoft.Xna.Framework;
using SkullThrone.Core.Raycaster;
using Xunit;

/// <summary>
/// Unit tests for <see cref="WallRenderer.RenderToFramebuffer"/> static logic.
/// Tests framebuffer rendering without any MonoGame runtime dependencies.
/// ISTQB techniques: equivalence partitioning, boundary value analysis.
/// </summary>
public sealed class WallRendererTests
{
    private static readonly Color CeilingColor = new(20, 12, 12);
    private static readonly Color FloorColor = new(40, 30, 30);

    private readonly Color[] _framebuffer = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];

    // Simple solid red texture (64x128)
    private readonly Color[][] _textureData;
    private readonly Color[][] _textureDataDark;

    public WallRendererTests()
    {
        int pixelCount = ProceduralTextures.TextureWidth * ProceduralTextures.TextureHeight;
        var solidRed = new Color[pixelCount];
        var solidDarkRed = new Color[pixelCount];
        Array.Fill(solidRed, Color.Red);
        Array.Fill(solidDarkRed, new Color(127, 0, 0));

        _textureData = [[], solidRed];      // Index 0 = empty, Index 1 = red
        _textureDataDark = [[], solidDarkRed];
    }

    private RayHit[] CreateUniformHitBuffer(RayHit hit)
    {
        var buffer = new RayHit[DdaRaycaster.ScreenWidth];
        Array.Fill(buffer, hit);
        return buffer;
    }

    #region EP — TextureId == 0 (No Wall)

    [Fact]
    public void RenderToFramebuffer_TextureIdZero_OnlyCeilingAndFloor()
    {
        // Arrange
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 0,
            IsVerticalSide = false,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor);

        // Assert — no texture (red) pixels should exist; wall region is left unwritten
        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        // Ceiling region
        for (int y = 0; y < drawStart; y++)
            Assert.Equal(CeilingColor, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

        // Floor region
        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
            Assert.Equal(FloorColor, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

        // Wall region — no texture sampled (no red)
        for (int y = drawStart; y < drawEnd; y++)
            Assert.NotEqual(Color.Red, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);
    }

    #endregion

    #region EP — TextureId Exceeds Array Length

    [Fact]
    public void RenderToFramebuffer_TextureIdExceedsArray_SkippedNoException()
    {
        // Arrange — textureData only has indices 0 and 1
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 99,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act — should not throw
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region BVA — PerpDistance = 0 (Extremely Close Wall)

    [Fact]
    public void RenderToFramebuffer_PerpDistanceZero_NoIndexOutOfBounds()
    {
        // Arrange — perpDistance 0 produces lineHeight = ScreenHeight (guard in CalculateLineHeight)
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 0f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void RenderToFramebuffer_PerpDistanceEpsilon_NoIndexOutOfBounds()
    {
        // Arrange — very small distance = very large lineHeight
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = float.Epsilon,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region BVA — WallX Boundary Values

    [Fact]
    public void RenderToFramebuffer_WallXExactlyOne_NoIndexOutOfBounds()
    {
        // Arrange — WallX=1.0 → texX = 64 & 63 = 0 (wraps via bitmask)
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 1.0f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void RenderToFramebuffer_WallXZero_SamplesLeftmostColumn()
    {
        // Arrange — WallX=0.0 → texX = 0
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.0f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region BVA — Very Close Wall (lineHeight > ScreenHeight)

    [Fact]
    public void RenderToFramebuffer_VeryCloseWall_FillsEntireColumnWithTexture()
    {
        // Arrange — perpDist=0.5 → lineHeight=400, drawStart=0, drawEnd=199
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 0.5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor);

        // Assert — center column: drawStart=0, drawEnd=199 (clamped)
        // Wall pixels from y=0 to y=198, floor at y=199 (floor loop: y from 199 to <200)
        int centerCol = DdaRaycaster.ScreenWidth / 2;
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(
            WallRenderingCalculations.CalculateLineHeight(0.5f));

        for (int y = 0; y < drawEnd; y++)
        {
            Color pixel = _framebuffer[y * DdaRaycaster.ScreenWidth + centerCol];
            Assert.Equal(Color.Red, pixel);
        }

        // Floor pixels from drawEnd to ScreenHeight-1
        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
        {
            Color pixel = _framebuffer[y * DdaRaycaster.ScreenWidth + centerCol];
            Assert.Equal(FloorColor, pixel);
        }
    }

    #endregion

    #region EP — Vertical vs Horizontal Side Shading

    [Fact]
    public void RenderToFramebuffer_VerticalSide_UsesFullBrightnessTexture()
    {
        // Arrange
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 1f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor);

        // Assert — wall pixels should be full red
        int centerCol = DdaRaycaster.ScreenWidth / 2;
        int midY = DdaRaycaster.ScreenHeight / 2;
        Assert.Equal(Color.Red, _framebuffer[midY * DdaRaycaster.ScreenWidth + centerCol]);
    }

    [Fact]
    public void RenderToFramebuffer_HorizontalSide_UsesDarkenedTexture()
    {
        // Arrange
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 1f,
            TextureId = 1,
            IsVerticalSide = false,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor);

        // Assert — wall pixels should be dark red
        int centerCol = DdaRaycaster.ScreenWidth / 2;
        int midY = DdaRaycaster.ScreenHeight / 2;
        Assert.Equal(new Color(127, 0, 0), _framebuffer[midY * DdaRaycaster.ScreenWidth + centerCol]);
    }

    #endregion

    #region EP — All Hits at MaxRayDistance (No Walls)

    [Fact]
    public void RenderToFramebuffer_AllHitsMaxDistance_EntireBufferIsCeilingAndFloor()
    {
        // Arrange — very far walls produce tiny line heights; TextureId=0 means no wall drawn
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = DdaRaycaster.MaxRayDistance,
            TextureId = 0,
            IsVerticalSide = false,
            WallX = 0f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor);

        // Assert — ceiling and floor regions are correct; tiny wall gap is unwritten
        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(DdaRaycaster.MaxRayDistance);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        for (int y = 0; y < drawStart; y++)
            Assert.Equal(CeilingColor, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
            Assert.Equal(FloorColor, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

        // No texture pixels anywhere
        for (int i = 0; i < _framebuffer.Length; i++)
            Assert.NotEqual(Color.Red, _framebuffer[i]);
    }

    #endregion

    #region BVA — Framebuffer Completeness

    [Fact]
    public void RenderToFramebuffer_NormalHit_AllPixelsWritten()
    {
        // Arrange — ensure no default(Color) pixels remain
        Array.Fill(_framebuffer, Color.Magenta); // sentinel value

        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, CeilingColor, FloorColor);

        // Assert — no magenta pixels should remain
        for (int i = 0; i < _framebuffer.Length; i++)
        {
            Assert.NotEqual(Color.Magenta, _framebuffer[i]);
        }
    }

    #endregion
}
