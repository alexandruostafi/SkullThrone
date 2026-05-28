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
    private static readonly Color UnwrittenPixel = default;

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
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

        // Assert — no texture (red) pixels should exist; wall region is left unwritten
        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        // Ceiling region — unwritten by wall renderer (handled by FloorCeilingRenderer)
        for (int y = 0; y < drawStart; y++)
            Assert.Equal(UnwrittenPixel, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

        // Floor region — unwritten by wall renderer (handled by FloorCeilingRenderer)
        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
            Assert.Equal(UnwrittenPixel, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

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
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

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
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

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
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

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
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

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
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

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
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

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

        // Floor pixels from drawEnd to ScreenHeight-1 — unwritten by wall renderer
        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
        {
            Color pixel = _framebuffer[y * DdaRaycaster.ScreenWidth + centerCol];
            Assert.Equal(UnwrittenPixel, pixel);
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
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

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
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

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
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

        // Assert — ceiling and floor regions are correct; tiny wall gap is unwritten
        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(DdaRaycaster.MaxRayDistance);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        for (int y = 0; y < drawStart; y++)
            Assert.Equal(UnwrittenPixel, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
            Assert.Equal(UnwrittenPixel, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);

        // No texture pixels anywhere
        for (int i = 0; i < _framebuffer.Length; i++)
            Assert.NotEqual(Color.Red, _framebuffer[i]);
    }

    #endregion

    #region BVA — Framebuffer Completeness

    [Fact]
    public void RenderToFramebuffer_NormalHit_WallPixelsWritten()
    {
        // Arrange — ensure wall region pixels get overwritten
        Array.Fill(_framebuffer, Color.Magenta); // sentinel value

        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

        // Assert — wall region pixels should be overwritten (not magenta)
        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        for (int y = drawStart; y < drawEnd; y++)
        {
            Assert.NotEqual(Color.Magenta, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);
        }

        // Floor/ceiling pixels remain magenta (handled by FloorCeilingRenderer)
        for (int y = 0; y < drawStart; y++)
            Assert.Equal(Color.Magenta, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);
    }

    #endregion

    #region EP — PerpDistance NaN and Infinity

    [Fact]
    public void RenderToFramebuffer_PerpDistanceNaN_NoException()
    {
        // Arrange — NaN from floating-point error should not crash
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = float.NaN,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void RenderToFramebuffer_PerpDistancePositiveInfinity_NoWallPixels()
    {
        // Arrange — Infinity → lineHeight=0, drawStart=drawEnd=100 → no wall loop
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = float.PositiveInfinity,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

        // Assert — no red (texture) pixels should exist
        for (int i = 0; i < _framebuffer.Length; i++)
        {
            Assert.NotEqual(Color.Red, _framebuffer[i]);
        }
    }

    #endregion

    #region EP — Negative WallX

    [Fact]
    public void RenderToFramebuffer_WallXNegative_NoException()
    {
        // Arrange — negative WallX wraps via bitmask: (int)(-0.5f * 64) & 63
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = -0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region BVA — Boundary Columns (0 and 319)

    [Fact]
    public void RenderToFramebuffer_Column0_WritesWallPixels()
    {
        // Arrange
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

        // Assert — column 0 should have wall pixels
        int midY = DdaRaycaster.ScreenHeight / 2;
        Assert.Equal(Color.Red, _framebuffer[midY * DdaRaycaster.ScreenWidth + 0]);
    }

    [Fact]
    public void RenderToFramebuffer_Column319_WritesWallPixels()
    {
        // Arrange
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

        // Assert — column 319 (last) should have wall pixels
        int midY = DdaRaycaster.ScreenHeight / 2;
        int lastCol = DdaRaycaster.ScreenWidth - 1;
        Assert.Equal(Color.Red, _framebuffer[midY * DdaRaycaster.ScreenWidth + lastCol]);
    }

    #endregion

    #region BVA — WallX Near 1.0

    [Fact]
    public void RenderToFramebuffer_WallXNearOne_SamplesLastTextureColumn()
    {
        // Arrange — WallX=0.999 → texX = (int)(0.999*64) & 63 = 63
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.999f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

        // Assert
        Assert.Null(exception);
        // Wall pixels should still be red (solid texture)
        int midY = DdaRaycaster.ScreenHeight / 2;
        int col = DdaRaycaster.ScreenWidth / 2;
        Assert.Equal(Color.Red, _framebuffer[midY * DdaRaycaster.ScreenWidth + col]);
    }

    #endregion

    #region EP — PerpDistance MaxValue

    [Fact]
    public void RenderToFramebuffer_PerpDistanceMaxValue_NoException()
    {
        // Arrange — float.MaxValue → lineHeight=0 → no wall pixels
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = float.MaxValue,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region EP — Non-Uniform Hit Buffer

    [Fact]
    public void RenderToFramebuffer_MixedHitBuffer_EachColumnRenderedIndependently()
    {
        // Arrange — left half close, right half far
        var hitBuffer = new RayHit[DdaRaycaster.ScreenWidth];
        for (int x = 0; x < DdaRaycaster.ScreenWidth / 2; x++)
        {
            hitBuffer[x] = new RayHit { PerpDistance = 0.5f, TextureId = 1, IsVerticalSide = true, WallX = 0.5f };
        }

        for (int x = DdaRaycaster.ScreenWidth / 2; x < DdaRaycaster.ScreenWidth; x++)
        {
            hitBuffer[x] = new RayHit { PerpDistance = 10f, TextureId = 1, IsVerticalSide = true, WallX = 0.5f };
        }

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark);

        // Assert — left column (close) should have wall pixel at top; right column (far) should not
        int topRow = 5;
        int leftCol = 10;
        int rightCol = DdaRaycaster.ScreenWidth - 10;

        // Close wall (0.5 distance) fills entire screen → red at top
        Assert.Equal(Color.Red, _framebuffer[topRow * DdaRaycaster.ScreenWidth + leftCol]);

        // Far wall (10 distance) → lineHeight=20, drawStart=90 → top row NOT wall
        Color rightPixel = _framebuffer[topRow * DdaRaycaster.ScreenWidth + rightCol];
        Assert.NotEqual(Color.Red, rightPixel);
    }

    #endregion

    #region EP — PitchOffset Positive (Looking Up)

    [Fact]
    public void RenderToFramebuffer_PositivePitch_WallShiftsDown()
    {
        // Arrange — pitchOffset=40 shifts walls down; top pixels become ceiling
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, pitchOffset: 40);

        // Assert — wall region shifted down; original center (y=100) should now be wall
        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight, 40);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight, 40);

        // Wall pixels in new range should be red
        int midWall = (drawStart + drawEnd) / 2;
        Assert.Equal(Color.Red, _framebuffer[midWall * DdaRaycaster.ScreenWidth + col]);

        // Pixels above drawStart should be unwritten (ceiling)
        if (drawStart > 0)
            Assert.Equal(Color.Magenta, _framebuffer[(drawStart - 1) * DdaRaycaster.ScreenWidth + col]);
    }

    #endregion

    #region EP — PitchOffset Negative (Looking Down)

    [Fact]
    public void RenderToFramebuffer_NegativePitch_WallShiftsUp()
    {
        // Arrange
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, pitchOffset: -40);

        // Assert
        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight, -40);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight, -40);

        int midWall = (drawStart + drawEnd) / 2;
        Assert.Equal(Color.Red, _framebuffer[midWall * DdaRaycaster.ScreenWidth + col]);

        // Pixels below drawEnd should be unwritten (floor)
        if (drawEnd < DdaRaycaster.ScreenHeight - 1)
            Assert.Equal(Color.Magenta, _framebuffer[(drawEnd + 1) * DdaRaycaster.ScreenWidth + col]);
    }

    #endregion

    #region BVA — PitchOffset at MaxPitch with Close Wall

    [Fact]
    public void RenderToFramebuffer_MaxPitchCloseWall_NoIndexOutOfBounds()
    {
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 0.5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, pitchOffset: 80));

        Assert.Null(exception);
    }

    [Fact]
    public void RenderToFramebuffer_MinPitchCloseWall_NoIndexOutOfBounds()
    {
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 0.5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, pitchOffset: -80));

        Assert.Null(exception);
    }

    #endregion

    #region EP — Texture Coordinate Correctness with PitchOffset

    [Fact]
    public void RenderToFramebuffer_PositivePitch_TextureNotGarbled()
    {
        // With a solid red texture, all wall pixels must be red regardless of pitchOffset
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 1f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, pitchOffset: 40);

        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(1f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight, 40);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight, 40);

        for (int y = drawStart; y < drawEnd; y++)
        {
            Assert.Equal(Color.Red, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);
        }
    }

    [Fact]
    public void RenderToFramebuffer_NegativePitch_TextureNotGarbled()
    {
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 1f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, pitchOffset: -40);

        int col = DdaRaycaster.ScreenWidth / 2;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(1f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight, -40);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight, -40);

        for (int y = drawStart; y < drawEnd; y++)
        {
            Assert.Equal(Color.Red, _framebuffer[y * DdaRaycaster.ScreenWidth + col]);
        }
    }

    #endregion

    #region BVA — PitchOffset with Far Wall

    [Fact]
    public void RenderToFramebuffer_MaxPitchFarWall_NoIndexOutOfBounds()
    {
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 20f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        var exception = Record.Exception(() =>
            WallRenderer.RenderToFramebuffer(hitBuffer, _framebuffer, _textureData, _textureDataDark, pitchOffset: 80));

        Assert.Null(exception);
    }

    #endregion
}
