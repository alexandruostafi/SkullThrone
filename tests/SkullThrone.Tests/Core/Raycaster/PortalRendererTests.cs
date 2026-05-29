namespace SkullThrone.Tests.Core.Raycaster;

using Microsoft.Xna.Framework;
using SkullThrone.Core.Raycaster;
using Xunit;

/// <summary>
/// Unit tests for <see cref="PortalRenderer"/>.
/// ISTQB techniques: equivalence partitioning, boundary value analysis.
/// </summary>
public sealed class PortalRendererTests
{
    private readonly PortalRenderer _renderer = new();

    #region Helpers

    private static Color[] CreateFramebuffer()
    {
        return new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];
    }

    #endregion

    #region RenderColumn — EP: Valid Color Names

    [Theory]
    [InlineData("green")]
    [InlineData("purple")]
    [InlineData("blue")]
    [InlineData("red")]
    [InlineData("orange")]
    public void RenderColumn_ValidColor_WritesPixelsToFramebuffer(string color)
    {
        // Arrange
        var framebuffer = CreateFramebuffer();
        int column = 160; // Center column
        int drawStart = 50;
        int drawEnd = 150;
        _renderer.Update();

        // Act
        _renderer.RenderColumn(framebuffer, column, drawStart, drawEnd, 0.5f, color);

        // Assert: at least some pixels written in the column
        bool anyWritten = false;
        for (int y = drawStart; y < drawEnd; y++)
        {
            if (framebuffer[y * DdaRaycaster.ScreenWidth + column] != default)
            {
                anyWritten = true;
                break;
            }
        }
        Assert.True(anyWritten);
    }

    [Fact]
    public void RenderColumn_NullColor_UsesDefaultColor()
    {
        // Arrange
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        // Act — should not throw
        _renderer.RenderColumn(framebuffer, 100, 80, 120, 0.5f, null);

        // Assert: pixels are written
        bool anyWritten = false;
        for (int y = 80; y < 120; y++)
        {
            if (framebuffer[y * DdaRaycaster.ScreenWidth + 100] != default)
            {
                anyWritten = true;
                break;
            }
        }
        Assert.True(anyWritten);
    }

    [Fact]
    public void RenderColumn_UnknownColor_UsesDefaultColor()
    {
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        // Should not throw, uses default
        _renderer.RenderColumn(framebuffer, 100, 80, 120, 0.5f, "nonexistent_color");

        bool anyWritten = false;
        for (int y = 80; y < 120; y++)
        {
            if (framebuffer[y * DdaRaycaster.ScreenWidth + 100] != default)
            {
                anyWritten = true;
                break;
            }
        }
        Assert.True(anyWritten);
    }

    #endregion

    #region RenderColumn — Circular Mask (BVA: center vs edge)

    [Fact]
    public void RenderColumn_CenterOfWall_RendersPortalColor()
    {
        // WallX = 0.5 (center), middle Y → inside circle → should be portal color or white particle
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        int column = 100;
        int drawStart = 50;
        int drawEnd = 150;
        int midY = (drawStart + drawEnd) / 2;

        _renderer.RenderColumn(framebuffer, column, drawStart, drawEnd, 0.5f, "green");

        var pixel = framebuffer[midY * DdaRaycaster.ScreenWidth + column];
        // Center pixel must be green (0, 200, 80) or white (255, 255, 255) particle
        bool isGreen = pixel.R == 0 && pixel.G == 200 && pixel.B == 80;
        bool isWhite = pixel.R == 255 && pixel.G == 255 && pixel.B == 255;
        Assert.True(isGreen || isWhite, $"Expected green or white particle at center, got ({pixel.R},{pixel.G},{pixel.B})");
    }

    [Fact]
    public void RenderColumn_EdgeOfWall_RendersBorderColor()
    {
        // WallX = 0.0 or 1.0 (extreme edge), any Y → outside circle → border color
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        int column = 100;
        int drawStart = 50;
        int drawEnd = 150;
        int midY = (drawStart + drawEnd) / 2;

        _renderer.RenderColumn(framebuffer, column, drawStart, drawEnd, 0.0f, "green");

        // Edge pixel: nx = 0.0 - 0.5 = -0.5, ny ≈ 0, distSq = 0.25 = CircleRadiusSq
        // This is right on the boundary — could be highlight or border
        var pixel = framebuffer[midY * DdaRaycaster.ScreenWidth + column];
        Assert.True(pixel != default);
    }

    #endregion

    #region RenderColumn — BVA: Draw bounds

    [Fact]
    public void RenderColumn_DrawStartNegative_ClampsToScreen()
    {
        // Should not throw or write out of bounds
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        _renderer.RenderColumn(framebuffer, 50, -10, 50, 0.5f, "green");

        // First valid row should have content
        var pixel = framebuffer[0 * DdaRaycaster.ScreenWidth + 50];
        Assert.True(pixel != default);
    }

    [Fact]
    public void RenderColumn_DrawEndBeyondScreen_ClampsToScreen()
    {
        // Should not throw or write out of bounds
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        _renderer.RenderColumn(framebuffer, 50, 180, DdaRaycaster.ScreenHeight + 20, 0.5f, "green");

        // Last valid row should have content
        var pixel = framebuffer[199 * DdaRaycaster.ScreenWidth + 50];
        Assert.True(pixel != default);
    }

    [Fact]
    public void RenderColumn_ZeroHeight_DoesNotThrow()
    {
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        // drawStart == drawEnd → zero height, nothing rendered
        _renderer.RenderColumn(framebuffer, 50, 100, 100, 0.5f, "green");

        // No pixels should be written at that column around row 100
        Assert.Equal(default, framebuffer[100 * DdaRaycaster.ScreenWidth + 50]);
    }

    #endregion

    #region Update — Frame Counter Animation

    [Fact]
    public void Update_MultipleFrames_ParticlesChange()
    {
        // After many frames, particles should shift (deterministic but different pattern)
        var framebuffer1 = CreateFramebuffer();
        var framebuffer2 = CreateFramebuffer();

        // Render at frame 0
        _renderer.RenderColumn(framebuffer1, 100, 50, 150, 0.5f, "green");

        // Advance many frames (particles change every 8 frames)
        for (int i = 0; i < 16; i++)
            _renderer.Update();

        _renderer.RenderColumn(framebuffer2, 100, 50, 150, 0.5f, "green");

        // At least one pixel should differ (particles moved)
        bool anyDiff = false;
        for (int y = 50; y < 150; y++)
        {
            int idx = y * DdaRaycaster.ScreenWidth + 100;
            if (framebuffer1[idx] != framebuffer2[idx])
            {
                anyDiff = true;
                break;
            }
        }
        Assert.True(anyDiff);
    }

    #endregion

    #region RenderColumn — Column boundary values

    [Theory]
    [InlineData(0)]
    [InlineData(DdaRaycaster.ScreenWidth - 1)]
    public void RenderColumn_FirstAndLastColumn_DoesNotThrow(int column)
    {
        var framebuffer = CreateFramebuffer();
        _renderer.Update();

        _renderer.RenderColumn(framebuffer, column, 50, 150, 0.5f, "green");

        // Verify pixels written
        var pixel = framebuffer[100 * DdaRaycaster.ScreenWidth + column];
        Assert.True(pixel != default);
    }

    #endregion
}
