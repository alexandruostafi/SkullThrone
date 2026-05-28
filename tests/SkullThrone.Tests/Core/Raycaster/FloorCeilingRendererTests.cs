namespace SkullThrone.Tests.Core.Raycaster;

using System;
using Microsoft.Xna.Framework;
using SkullThrone.Core.Raycaster;
using Xunit;

/// <summary>
/// Unit tests for <see cref="FloorCeilingRenderer"/>.
/// Tests the per-pixel floor/ceiling rendering logic including texture mapping and fog.
/// </summary>
public sealed class FloorCeilingRendererTests
{
    private readonly Color[] _framebuffer = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];

    private static RayHit[] CreateUniformHitBuffer(RayHit hit)
    {
        var buffer = new RayHit[DdaRaycaster.ScreenWidth];
        Array.Fill(buffer, hit);
        return buffer;
    }

    #region Floor Rendering

    [Fact]
    public void Render_FloorRegion_WritesNonDefaultPixels()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — floor pixels (below wall) should be overwritten
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);
        int col = DdaRaycaster.ScreenWidth / 2;

        bool anyFloorWritten = false;
        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
        {
            if (_framebuffer[y * DdaRaycaster.ScreenWidth + col] != Color.Magenta)
            {
                anyFloorWritten = true;
                break;
            }
        }

        Assert.True(anyFloorWritten);
    }

    [Fact]
    public void Render_FloorRegion_PixelsAreDark()
    {
        // Arrange — dark brick floor should produce dark pixels
        var renderer = new FloorCeilingRenderer(floorTextureId: ProceduralFloorCeilingTextures.FloorDarkBricks);
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — sample floor pixels, they should be dark
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);
        int col = DdaRaycaster.ScreenWidth / 2;
        int sampleY = (drawEnd + DdaRaycaster.ScreenHeight) / 2;

        Color pixel = _framebuffer[sampleY * DdaRaycaster.ScreenWidth + col];
        int brightness = pixel.R + pixel.G + pixel.B;
        Assert.True(brightness < 200, $"Floor pixel too bright: R={pixel.R} G={pixel.G} B={pixel.B}");
    }

    #endregion

    #region Ceiling Rendering

    [Fact]
    public void Render_CeilingRegion_WritesNonDefaultPixels()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — ceiling pixels (above wall) should be overwritten
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int col = DdaRaycaster.ScreenWidth / 2;

        bool anyCeilingWritten = false;
        for (int y = 0; y < drawStart; y++)
        {
            if (_framebuffer[y * DdaRaycaster.ScreenWidth + col] != Color.Magenta)
            {
                anyCeilingWritten = true;
                break;
            }
        }

        Assert.True(anyCeilingWritten);
    }

    [Fact]
    public void Render_CeilingDungeon_ProducesDarkPixels()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer(ceilingTextureId: ProceduralFloorCeilingTextures.CeilingDungeon);
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — sample ceiling pixel, dungeon is dark
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int col = DdaRaycaster.ScreenWidth / 2;
        int sampleY = drawStart / 2;

        if (sampleY > 0)
        {
            Color pixel = _framebuffer[sampleY * DdaRaycaster.ScreenWidth + col];
            int brightness = pixel.R + pixel.G + pixel.B;
            Assert.True(brightness < 200, $"Ceiling pixel too bright: R={pixel.R} G={pixel.G} B={pixel.B}");
        }
    }

    #endregion

    #region Fog

    [Fact]
    public void Render_FarFloorPixels_AreDarkerThanNearPixels()
    {
        // Arrange — wall far away so floor takes up most of the screen
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 20f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — bottom row (near) should be brighter than rows closer to horizon (far)
        int col = DdaRaycaster.ScreenWidth / 2;
        int halfHeight = DdaRaycaster.ScreenHeight / 2;

        // Near pixel (bottom of screen)
        Color nearPixel = _framebuffer[(DdaRaycaster.ScreenHeight - 2) * DdaRaycaster.ScreenWidth + col];
        // Far pixel (just below horizon)
        Color farPixel = _framebuffer[(halfHeight + 2) * DdaRaycaster.ScreenWidth + col];

        int nearBrightness = nearPixel.R + nearPixel.G + nearPixel.B;
        int farBrightness = farPixel.R + farPixel.G + farPixel.B;

        // Near should be brighter or equal (fog darkens far pixels)
        Assert.True(nearBrightness >= farBrightness,
            $"Near brightness ({nearBrightness}) should be >= far brightness ({farBrightness})");
    }

    #endregion

    #region Wall Region Not Overwritten

    [Fact]
    public void Render_WallRegion_NotOverwritten()
    {
        // Arrange — fill framebuffer with sentinel, render only floor/ceiling
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 2f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — wall region pixels should remain magenta (untouched)
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(2f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);
        int col = DdaRaycaster.ScreenWidth / 2;

        // Sample middle of wall region
        int wallMid = (drawStart + drawEnd) / 2;
        Assert.Equal(Color.Magenta, _framebuffer[wallMid * DdaRaycaster.ScreenWidth + col]);
    }

    #endregion

    #region Player Position Affects Texturing

    [Fact]
    public void Render_DifferentPlayerPositions_ProduceDifferentPixels()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        var framebuffer1 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];
        var framebuffer2 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];

        // Act
        renderer.Render(framebuffer1, hitBuffer, 8f, 8f, 0f);
        renderer.Render(framebuffer2, hitBuffer, 10f, 10f, 0f);

        // Assert — at least some floor pixels differ
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(5f);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        bool hasDifference = false;
        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
        {
            for (int x = 0; x < DdaRaycaster.ScreenWidth; x++)
            {
                int idx = y * DdaRaycaster.ScreenWidth + x;
                if (framebuffer1[idx] != framebuffer2[idx])
                {
                    hasDifference = true;
                    break;
                }
            }

            if (hasDifference) break;
        }

        Assert.True(hasDifference);
    }

    [Fact]
    public void Render_DifferentPlayerAngles_ProduceDifferentPixels()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        var framebuffer1 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];
        var framebuffer2 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];

        // Act
        renderer.Render(framebuffer1, hitBuffer, 8f, 8f, 0f);
        renderer.Render(framebuffer2, hitBuffer, 8f, 8f, MathF.PI / 2f);

        // Assert
        bool hasDifference = false;
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(5f);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);

        for (int y = drawEnd; y < DdaRaycaster.ScreenHeight; y++)
        {
            int idx = y * DdaRaycaster.ScreenWidth + DdaRaycaster.ScreenWidth / 2;
            if (framebuffer1[idx] != framebuffer2[idx])
            {
                hasDifference = true;
                break;
            }
        }

        Assert.True(hasDifference);
    }

    #endregion

    #region Determinism

    [Fact]
    public void Render_SameInputs_ProducesIdenticalOutput()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 3f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        var framebuffer1 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];
        var framebuffer2 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];

        // Act
        renderer.Render(framebuffer1, hitBuffer, 8f, 8f, 0f);
        renderer.Render(framebuffer2, hitBuffer, 8f, 8f, 0f);

        // Assert
        Assert.Equal(framebuffer1, framebuffer2);
    }

    #endregion

    #region Texture Selection

    [Fact]
    public void Render_SpaceCeiling_ProducesDifferentPixelsThanDungeon()
    {
        // Arrange
        var spaceRenderer = new FloorCeilingRenderer(ceilingTextureId: ProceduralFloorCeilingTextures.CeilingSpace);
        var dungeonRenderer = new FloorCeilingRenderer(ceilingTextureId: ProceduralFloorCeilingTextures.CeilingDungeon);
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        var framebuffer1 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];
        var framebuffer2 = new Color[DdaRaycaster.ScreenWidth * DdaRaycaster.ScreenHeight];

        // Act
        spaceRenderer.Render(framebuffer1, hitBuffer, 8f, 8f, 0f);
        dungeonRenderer.Render(framebuffer2, hitBuffer, 8f, 8f, 0f);

        // Assert — ceiling regions differ
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(5f);
        int drawStart = WallRenderingCalculations.CalculateDrawStart(lineHeight);

        bool hasDifference = false;
        for (int y = 0; y < drawStart; y++)
        {
            int idx = y * DdaRaycaster.ScreenWidth + DdaRaycaster.ScreenWidth / 2;
            if (framebuffer1[idx] != framebuffer2[idx])
            {
                hasDifference = true;
                break;
            }
        }

        Assert.True(hasDifference);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Render_WallFillsEntireScreen_AlmostNoFloorCeilingWritten()
    {
        // Arrange — very close wall fills almost entire screen
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 0.1f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — nearly all pixels should remain magenta (wall covers screen)
        int magentaCount = 0;
        for (int i = 0; i < _framebuffer.Length; i++)
        {
            if (_framebuffer[i] == Color.Magenta)
                magentaCount++;
        }

        float magentaRatio = (float)magentaCount / _framebuffer.Length;
        Assert.True(magentaRatio > 0.95f, $"Expected >95% untouched, got {magentaRatio:P}");
    }

    [Fact]
    public void Render_WallAtMaxDistance_MostPixelsAreFloorCeiling()
    {
        // Arrange — wall far away, most of screen is floor/ceiling
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = DdaRaycaster.MaxRayDistance,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });
        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — most pixels should be overwritten
        int magentaCount = 0;
        for (int i = 0; i < _framebuffer.Length; i++)
        {
            if (_framebuffer[i] == Color.Magenta)
                magentaCount++;
        }

        float writtenRatio = 1f - (float)magentaCount / _framebuffer.Length;
        Assert.True(writtenRatio > 0.9f, $"Expected >90% written, got {writtenRatio:P}");
    }

    #endregion

    #region BVA — Negative and Zero PerpDistance

    [Fact]
    public void Render_PerpDistanceZero_NoException()
    {
        // Arrange — PerpDistance=0 should not crash renderer
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 0f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Render_PerpDistanceNegative_NoException()
    {
        // Arrange — negative PerpDistance from floating-point error should not crash
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = -1f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region BVA — Horizon Boundary Rows

    [Fact]
    public void Render_FirstFloorRow_ProducesValidColor()
    {
        // Arrange — wall at max distance so drawEnd is at/near horizon
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = DdaRaycaster.MaxRayDistance,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — find the first floor row that's actually below drawEnd
        int lineHeight = WallRenderingCalculations.CalculateLineHeight(DdaRaycaster.MaxRayDistance);
        int drawEnd = WallRenderingCalculations.CalculateDrawEnd(lineHeight);
        int halfHeight = DdaRaycaster.ScreenHeight / 2;
        int firstFloorRow = Math.Max(drawEnd, halfHeight + 1);
        int col = DdaRaycaster.ScreenWidth / 2;
        Color pixel = _framebuffer[firstFloorRow * DdaRaycaster.ScreenWidth + col];

        // Should be rendered (not default)
        Assert.NotEqual(default(Color), pixel);
        Assert.Equal(255, pixel.A);
    }

    [Fact]
    public void Render_LastFloorRow_ProducesValidColor()
    {
        // Arrange — wall far away
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 50f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — y=199 (last row) should be valid, near pixel with minimal fog
        int lastRow = DdaRaycaster.ScreenHeight - 1;
        int col = DdaRaycaster.ScreenWidth / 2;
        Color pixel = _framebuffer[lastRow * DdaRaycaster.ScreenWidth + col];

        Assert.NotEqual(default(Color), pixel);
        Assert.Equal(255, pixel.A);
    }

    #endregion

    #region EP — Negative Player Angle

    [Fact]
    public void Render_NegativePlayerAngle_NoException()
    {
        // Arrange — MathF.Cos/Sin handle negatives, but verify no crash
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            renderer.Render(_framebuffer, hitBuffer, 8f, 8f, -MathF.PI));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Render_PlayerAngleGreaterThan2Pi_NoException()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 3f * MathF.PI));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region EP — Player Position Edge Values

    [Fact]
    public void Render_PlayerAtOrigin_NoException()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            renderer.Render(_framebuffer, hitBuffer, 0f, 0f, 0f));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Render_PlayerNegativePosition_NoException()
    {
        // Arrange
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 5f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            renderer.Render(_framebuffer, hitBuffer, -5f, -5f, 0f));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region EP — Non-Uniform Hit Buffer

    [Fact]
    public void Render_NonUniformHitBuffer_LeftCloseRightFar_ProducesAsymmetricOutput()
    {
        // Arrange — left half close wall, right half far wall
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = new RayHit[DdaRaycaster.ScreenWidth];

        for (int x = 0; x < DdaRaycaster.ScreenWidth / 2; x++)
        {
            hitBuffer[x] = new RayHit { PerpDistance = 1f, TextureId = 1, IsVerticalSide = true, WallX = 0.5f };
        }

        for (int x = DdaRaycaster.ScreenWidth / 2; x < DdaRaycaster.ScreenWidth; x++)
        {
            hitBuffer[x] = new RayHit { PerpDistance = 10f, TextureId = 1, IsVerticalSide = true, WallX = 0.5f };
        }

        Array.Fill(_framebuffer, Color.Magenta);

        // Act
        renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f);

        // Assert — right side (far wall) should have more floor pixels than left side (close wall)
        int leftFloorPixels = 0;
        int rightFloorPixels = 0;

        // Count non-magenta pixels in bottom quarter for each half
        int checkStart = DdaRaycaster.ScreenHeight * 3 / 4;
        for (int y = checkStart; y < DdaRaycaster.ScreenHeight; y++)
        {
            // Left column
            if (_framebuffer[y * DdaRaycaster.ScreenWidth + 10] != Color.Magenta)
                leftFloorPixels++;
            // Right column
            if (_framebuffer[y * DdaRaycaster.ScreenWidth + DdaRaycaster.ScreenWidth - 10] != Color.Magenta)
                rightFloorPixels++;
        }

        // Right side has far wall → more visible floor
        Assert.True(rightFloorPixels >= leftFloorPixels,
            $"Right floor pixels ({rightFloorPixels}) should be >= left ({leftFloorPixels})");
    }

    #endregion

    #region BVA — PerpDistance = 1.0 Exactly

    [Fact]
    public void Render_PerpDistanceExactlyOne_NoException()
    {
        // Arrange — lineHeight=200, drawStart=0, drawEnd=199 → edge case
        var renderer = new FloorCeilingRenderer();
        var hitBuffer = CreateUniformHitBuffer(new RayHit
        {
            PerpDistance = 1.0f,
            TextureId = 1,
            IsVerticalSide = true,
            WallX = 0.5f
        });

        // Act
        var exception = Record.Exception(() =>
            renderer.Render(_framebuffer, hitBuffer, 8f, 8f, 0f));

        // Assert
        Assert.Null(exception);
    }

    #endregion
}
