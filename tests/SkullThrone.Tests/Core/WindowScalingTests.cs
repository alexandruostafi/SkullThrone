namespace SkullThrone.Tests.Core;

using Microsoft.Xna.Framework;
using Xunit;

/// <summary>
/// Tests for window scaling letterbox/pillarbox calculations.
/// </summary>
public sealed class WindowScalingTests
{
    [Fact]
    public void CalculateLetterboxRect_ExactMultiple_FillsEntireWindow()
    {
        // 3x scale: 960×600
        var rect = SkullThroneGame.CalculateLetterboxRect(960, 600);

        Assert.Equal(0, rect.X);
        Assert.Equal(0, rect.Y);
        Assert.Equal(960, rect.Width);
        Assert.Equal(600, rect.Height);
    }

    [Fact]
    public void CalculateLetterboxRect_WiderWindow_PillarboxesCentered()
    {
        // Window is wider than 16:10 — should pillarbox (bars on sides)
        var rect = SkullThroneGame.CalculateLetterboxRect(1200, 600);

        Assert.Equal(600, rect.Height);
        Assert.Equal(960, rect.Width); // height-limited: 600/200=3x → 320*3=960
        Assert.Equal((1200 - 960) / 2, rect.X);
        Assert.Equal(0, rect.Y);
    }

    [Fact]
    public void CalculateLetterboxRect_TallerWindow_LetterboxesCentered()
    {
        // Window is taller than 16:10 — should letterbox (bars on top/bottom)
        var rect = SkullThroneGame.CalculateLetterboxRect(960, 800);

        Assert.Equal(960, rect.Width); // width-limited: 960/320=3x → 200*3=600
        Assert.Equal(600, rect.Height);
        Assert.Equal(0, rect.X);
        Assert.Equal((800 - 600) / 2, rect.Y);
    }

    [Theory]
    [InlineData(320, 200)]
    [InlineData(640, 400)]
    [InlineData(1280, 800)]
    [InlineData(1920, 1200)]
    public void CalculateLetterboxRect_IntegerMultiples_NoMargins(int width, int height)
    {
        var rect = SkullThroneGame.CalculateLetterboxRect(width, height);

        Assert.Equal(0, rect.X);
        Assert.Equal(0, rect.Y);
        Assert.Equal(width, rect.Width);
        Assert.Equal(height, rect.Height);
    }

    [Fact]
    public void CalculateLetterboxRect_FullHD_MaintainsAspectRatio()
    {
        // 1920×1080 — height-limited (1080/200=5.4x → width=320*5.4=1728, height=1080)
        var rect = SkullThroneGame.CalculateLetterboxRect(1920, 1080);

        float aspectRatio = (float)rect.Width / rect.Height;
        float expected = (float)SkullThroneGame.LogicalWidth / SkullThroneGame.LogicalHeight;

        Assert.Equal(expected, aspectRatio, precision: 3);
        Assert.True(rect.Width <= 1920);
        Assert.True(rect.Height <= 1080);
    }

    [Fact]
    public void CalculateLetterboxRect_SmallerThanLogical_ScalesDown()
    {
        // Window smaller than 320×200 — should scale below 1x
        var rect = SkullThroneGame.CalculateLetterboxRect(160, 100);

        Assert.Equal(160, rect.Width); // 0.5x scale
        Assert.Equal(100, rect.Height);
        Assert.Equal(0, rect.X);
        Assert.Equal(0, rect.Y);
    }

    [Fact]
    public void CalculateLetterboxRect_AlwaysCenteredInWindow()
    {
        var rect = SkullThroneGame.CalculateLetterboxRect(1366, 768);

        // Verify centering: margins on both sides should be equal
        int leftMargin = rect.X;
        int rightMargin = 1366 - rect.X - rect.Width;
        int topMargin = rect.Y;
        int bottomMargin = 768 - rect.Y - rect.Height;

        Assert.Equal(leftMargin, rightMargin);
        Assert.Equal(topMargin, bottomMargin);
    }
}
