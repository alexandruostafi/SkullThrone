namespace SkullThrone.Tests.Core.Raycaster;

using Microsoft.Xna.Framework;
using SkullThrone.Core.Raycaster;
using Xunit;

/// <summary>
/// Unit tests for <see cref="WallRenderingCalculations"/>.
/// Tests the pure calculation logic extracted from WallRenderer.
/// ISTQB techniques: equivalence partitioning, boundary value analysis.
/// </summary>
public sealed class WallRenderingCalculationsTests
{
    #region CalculateLineHeight

    [Theory]
    [InlineData(1.0f, 200)]   // Distance 1 → full screen height
    [InlineData(2.0f, 100)]   // Distance 2 → half height
    [InlineData(4.0f, 50)]    // Distance 4 → quarter height
    [InlineData(0.5f, 400)]   // Very close → taller than screen
    public void CalculateLineHeight_ValidDistances_ReturnsExpected(float perpDist, int expected)
    {
        Assert.Equal(expected, WallRenderingCalculations.CalculateLineHeight(perpDist));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(-0.001f)]
    public void CalculateLineHeight_ZeroOrNegativeDistance_ReturnsScreenHeight(float perpDist)
    {
        Assert.Equal(DdaRaycaster.ScreenHeight, WallRenderingCalculations.CalculateLineHeight(perpDist));
    }

    [Fact]
    public void CalculateLineHeight_VeryLargeDistance_ReturnsSmallHeight()
    {
        int height = WallRenderingCalculations.CalculateLineHeight(64f);
        Assert.True(height < 10);
        Assert.True(height > 0);
    }

    [Fact]
    public void CalculateLineHeight_VerySmallDistance_ReturnsLargeHeight()
    {
        int height = WallRenderingCalculations.CalculateLineHeight(0.01f);
        Assert.True(height > DdaRaycaster.ScreenHeight);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.NegativeInfinity)]
    public void CalculateLineHeight_NaNOrNegativeInfinity_ReturnsScreenHeight(float perpDist)
    {
        // NaN: !(NaN > 0) is true → guard returns ScreenHeight
        // -∞: !(-∞ > 0) is true → guard returns ScreenHeight
        Assert.Equal(DdaRaycaster.ScreenHeight, WallRenderingCalculations.CalculateLineHeight(perpDist));
    }

    [Fact]
    public void CalculateLineHeight_PositiveInfinity_ReturnsZero()
    {
        // +∞ > 0 is true, so guard does NOT catch it → (int)(200 / ∞) = 0
        Assert.Equal(0, WallRenderingCalculations.CalculateLineHeight(float.PositiveInfinity));
    }

    [Fact]
    public void CalculateLineHeight_FloatEpsilon_ReturnsLargeHeight()
    {
        int height = WallRenderingCalculations.CalculateLineHeight(float.Epsilon);
        Assert.True(height > DdaRaycaster.ScreenHeight);
    }

    #endregion

    #region CalculateDrawStart — BVA

    [Fact]
    public void CalculateDrawStart_SmallLineHeight_ReturnsPositiveValue()
    {
        // lineHeight=50 → drawStart = -25 + 100 = 75
        Assert.Equal(75, WallRenderingCalculations.CalculateDrawStart(50));
    }

    [Fact]
    public void CalculateDrawStart_LineHeightEqualsScreenHeight_ReturnsZero()
    {
        // lineHeight=200 → drawStart = -100 + 100 = 0
        Assert.Equal(0, WallRenderingCalculations.CalculateDrawStart(200));
    }

    [Fact]
    public void CalculateDrawStart_LineHeightExceedsScreen_ClampedToZero()
    {
        // lineHeight=400 → drawStart = -200 + 100 = -100 → clamped to 0
        Assert.Equal(0, WallRenderingCalculations.CalculateDrawStart(400));
    }

    [Fact]
    public void CalculateDrawStart_ZeroLineHeight_ReturnsCenterOfScreen()
    {
        // lineHeight=0 → drawStart = 0 + 100 = 100
        Assert.Equal(DdaRaycaster.ScreenHeight / 2, WallRenderingCalculations.CalculateDrawStart(0));
    }

    [Fact]
    public void CalculateDrawStart_NegativeLineHeight_ReturnsAboveCenter()
    {
        // lineHeight=-10 → drawStart = 5 + 100 = 105
        int start = WallRenderingCalculations.CalculateDrawStart(-10);
        Assert.True(start > DdaRaycaster.ScreenHeight / 2);
    }

    [Fact]
    public void CalculateDrawStart_LineHeight199_ReturnsBoundaryValue()
    {
        // lineHeight=199 → drawStart = -99 + 100 = 1
        Assert.Equal(1, WallRenderingCalculations.CalculateDrawStart(199));
    }

    [Fact]
    public void CalculateDrawStart_LineHeight201_ClampedToZero()
    {
        // lineHeight=201 → drawStart = -100 + 100 = 0 (or -1 + 100 depending on truncation)
        int start = WallRenderingCalculations.CalculateDrawStart(201);
        Assert.Equal(0, start);
    }

    #endregion

    #region CalculateDrawEnd — BVA

    [Fact]
    public void CalculateDrawEnd_SmallLineHeight_ReturnsValueBelowCenter()
    {
        // lineHeight=50 → drawEnd = 25 + 100 = 125
        Assert.Equal(125, WallRenderingCalculations.CalculateDrawEnd(50));
    }

    [Fact]
    public void CalculateDrawEnd_LineHeightEqualsScreenHeight_ReturnsMaxPixel()
    {
        // lineHeight=200 → drawEnd = 100 + 100 = 200 → clamped to 199
        Assert.Equal(DdaRaycaster.ScreenHeight - 1, WallRenderingCalculations.CalculateDrawEnd(200));
    }

    [Fact]
    public void CalculateDrawEnd_LineHeightExceedsScreen_ClampedToMaxPixel()
    {
        // lineHeight=400 → drawEnd = 200 + 100 = 300 → clamped to 199
        Assert.Equal(DdaRaycaster.ScreenHeight - 1, WallRenderingCalculations.CalculateDrawEnd(400));
    }

    [Fact]
    public void CalculateDrawEnd_ZeroLineHeight_ReturnsCenterOfScreen()
    {
        // lineHeight=0 → drawEnd = 0 + 100 = 100
        Assert.Equal(DdaRaycaster.ScreenHeight / 2, WallRenderingCalculations.CalculateDrawEnd(0));
    }

    [Fact]
    public void CalculateDrawEnd_NegativeLineHeight_ReturnsBelowCenter()
    {
        // lineHeight=-10 → drawEnd = -5 + 100 = 95
        int end = WallRenderingCalculations.CalculateDrawEnd(-10);
        Assert.True(end < DdaRaycaster.ScreenHeight / 2);
    }

    [Fact]
    public void CalculateDrawEnd_LineHeight199_ReturnsBoundaryValue()
    {
        // lineHeight=199 → drawEnd = 99 + 100 = 199 (not clamped, naturally at max valid pixel)
        Assert.Equal(199, WallRenderingCalculations.CalculateDrawEnd(199));
    }

    #endregion

    #region Draw Range Consistency

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(200)]
    [InlineData(500)]
    [InlineData(1000)]
    public void DrawStartAndEnd_AnyLineHeight_StartLessThanOrEqualToEnd(int lineHeight)
    {
        int start = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int end = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        Assert.True(start <= end, $"Start ({start}) > End ({end}) for lineHeight={lineHeight}");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void DrawStartAndEnd_AnyLineHeight_WithinScreenBounds(int lineHeight)
    {
        int start = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int end = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        Assert.InRange(start, 0, DdaRaycaster.ScreenHeight - 1);
        Assert.InRange(end, 0, DdaRaycaster.ScreenHeight - 1);
    }

    #endregion

    #region GetWallColor — EP

    private static readonly Color[] TestWallColors =
    [
        Color.Black,
        Color.DarkRed,
        Color.DarkGray,
        Color.DarkGoldenrod,
        Color.DarkSlateGray
    ];

    [Fact]
    public void GetWallColor_ValidTextureId_VerticalSide_ReturnsFullColor()
    {
        var color = WallRenderingCalculations.GetWallColor(1, isVerticalSide: true, TestWallColors);
        Assert.Equal(Color.DarkRed, color);
    }

    [Fact]
    public void GetWallColor_ValidTextureId_HorizontalSide_ReturnsDarkenedColor()
    {
        var color = WallRenderingCalculations.GetWallColor(1, isVerticalSide: false, TestWallColors);

        // DarkRed = (139, 0, 0) → darkened: (83, 0, 0)
        Assert.Equal((byte)(139 * 0.6f), color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void GetWallColor_TextureIdExceedsArray_FallsBackToIndex1()
    {
        var color = WallRenderingCalculations.GetWallColor(99, isVerticalSide: true, TestWallColors);
        Assert.Equal(Color.DarkRed, color); // Index 1 fallback
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void GetWallColor_AllValidTextureIds_DoNotThrow(int textureId)
    {
        var color = WallRenderingCalculations.GetWallColor(textureId, isVerticalSide: true, TestWallColors);
        Assert.NotEqual(Color.Black, color); // Index 0 is "no wall"
    }

    [Fact]
    public void GetWallColor_HorizontalSide_AlwaysDarkerThanVertical()
    {
        for (int id = 1; id < TestWallColors.Length; id++)
        {
            var vertical = WallRenderingCalculations.GetWallColor(id, isVerticalSide: true, TestWallColors);
            var horizontal = WallRenderingCalculations.GetWallColor(id, isVerticalSide: false, TestWallColors);

            Assert.True(horizontal.R <= vertical.R, $"TextureId {id}: horizontal R not darker");
            Assert.True(horizontal.G <= vertical.G, $"TextureId {id}: horizontal G not darker");
            Assert.True(horizontal.B <= vertical.B, $"TextureId {id}: horizontal B not darker");
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(0)]
    public void GetWallColor_NegativeOrZeroTextureId_FallsBackToIndex1(int textureId)
    {
        // Negative and zero textureIds should not crash — they fall back to index 1
        var color = WallRenderingCalculations.GetWallColor(textureId, isVerticalSide: true, TestWallColors);
        Assert.Equal(Color.DarkRed, color);
    }

    [Fact]
    public void GetWallColor_BoundaryTextureId5_FallsBackToIndex1()
    {
        // First out-of-bounds value (array has 5 elements, indices 0-4)
        var color = WallRenderingCalculations.GetWallColor(5, isVerticalSide: true, TestWallColors);
        Assert.Equal(Color.DarkRed, color);
    }

    #endregion

    #region CalculateDrawStart with pitchOffset — EP

    [Theory]
    [InlineData(50, 40, 115)]   // -25+100+40=115
    [InlineData(50, -40, 35)]   // -25+100-40=35
    [InlineData(200, 80, 80)]   // -100+100+80=80
    [InlineData(200, -80, 0)]   // -100+100-80=-80→0
    public void CalculateDrawStart_WithPitchOffset_ReturnsExpected(int lineHeight, int pitchOffset, int expected)
    {
        Assert.Equal(expected, WallRenderingCalculations.CalculateDrawStart(lineHeight, pitchOffset));
    }

    [Theory]
    [InlineData(400, -80)]  // -200+100-80=-180→0
    [InlineData(400, 80)]   // -200+100+80=-20→0
    public void CalculateDrawStart_LargeLineHeightWithPitch_ClampedToZero(int lineHeight, int pitchOffset)
    {
        Assert.Equal(0, WallRenderingCalculations.CalculateDrawStart(lineHeight, pitchOffset));
    }

    [Theory]
    [InlineData(10, 80)]  // -5+100+80=175
    [InlineData(10, -80)] // -5+100-80=15
    public void CalculateDrawStart_SmallLineHeightWithPitch_WithinBounds(int lineHeight, int pitchOffset)
    {
        int start = WallRenderingCalculations.CalculateDrawStart(lineHeight, pitchOffset);
        Assert.InRange(start, 0, DdaRaycaster.ScreenHeight - 1);
    }

    #endregion

    #region CalculateDrawEnd with pitchOffset — EP

    [Theory]
    [InlineData(50, 40, 165)]    // 25+100+40=165
    [InlineData(50, -40, 85)]    // 25+100-40=85
    [InlineData(200, 80, 199)]   // 100+100+80=280→199
    [InlineData(200, -80, 120)]  // 100+100-80=120
    public void CalculateDrawEnd_WithPitchOffset_ReturnsExpected(int lineHeight, int pitchOffset, int expected)
    {
        Assert.Equal(expected, WallRenderingCalculations.CalculateDrawEnd(lineHeight, pitchOffset));
    }

    [Theory]
    [InlineData(200, 80)]  // clamped
    [InlineData(400, 80)]  // clamped
    [InlineData(400, -80)] // 200+100-80=220→199
    public void CalculateDrawEnd_LargeLineHeightWithPitch_ClampedToMax(int lineHeight, int pitchOffset)
    {
        Assert.Equal(DdaRaycaster.ScreenHeight - 1, WallRenderingCalculations.CalculateDrawEnd(lineHeight, pitchOffset));
    }

    [Theory]
    [InlineData(10, -80)] // 5+100-80=25
    [InlineData(10, 80)]  // 5+100+80=185
    public void CalculateDrawEnd_SmallLineHeightWithPitch_WithinBounds(int lineHeight, int pitchOffset)
    {
        int end = WallRenderingCalculations.CalculateDrawEnd(lineHeight, pitchOffset);
        Assert.InRange(end, 0, DdaRaycaster.ScreenHeight - 1);
    }

    #endregion

    #region Draw Range Consistency with pitchOffset

    [Theory]
    [InlineData(10, 80)]
    [InlineData(10, -80)]
    [InlineData(50, 40)]
    [InlineData(50, -40)]
    [InlineData(200, 80)]
    [InlineData(200, -80)]
    [InlineData(0, 80)]
    [InlineData(0, -80)]
    [InlineData(500, 0)]
    public void DrawStartAndEnd_WithPitchOffset_StartLessThanOrEqualToEnd(int lineHeight, int pitchOffset)
    {
        int start = WallRenderingCalculations.CalculateDrawStart(lineHeight, pitchOffset);
        int end = WallRenderingCalculations.CalculateDrawEnd(lineHeight, pitchOffset);
        Assert.True(start <= end, $"Start ({start}) > End ({end}) for lineHeight={lineHeight}, pitch={pitchOffset}");
    }

    [Theory]
    [InlineData(10, 80)]
    [InlineData(10, -80)]
    [InlineData(200, 80)]
    [InlineData(200, -80)]
    [InlineData(50, 0)]
    public void DrawStartAndEnd_WithPitchOffset_WithinScreenBounds(int lineHeight, int pitchOffset)
    {
        int start = WallRenderingCalculations.CalculateDrawStart(lineHeight, pitchOffset);
        int end = WallRenderingCalculations.CalculateDrawEnd(lineHeight, pitchOffset);
        Assert.InRange(start, 0, DdaRaycaster.ScreenHeight - 1);
        Assert.InRange(end, 0, DdaRaycaster.ScreenHeight - 1);
    }

    #endregion
}
