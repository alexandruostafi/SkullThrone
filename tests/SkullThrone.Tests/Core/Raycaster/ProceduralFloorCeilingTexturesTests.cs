namespace SkullThrone.Tests.Core.Raycaster;

using Microsoft.Xna.Framework;
using SkullThrone.Core.Raycaster;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ProceduralFloorCeilingTextures"/> procedural texture generation.
/// Verifies texture dimensions, pixel validity, and visual properties.
/// </summary>
public sealed class ProceduralFloorCeilingTexturesTests
{
    #region Floor Textures

    [Fact]
    public void GenerateFloorTextures_ReturnsNonEmptyArray()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateFloorTextures();

        // Assert
        Assert.NotEmpty(textures);
    }

    [Fact]
    public void GenerateFloorTextures_DarkBricks_HasCorrectPixelCount()
    {
        // Arrange
        int expected = ProceduralFloorCeilingTextures.TextureSize * ProceduralFloorCeilingTextures.TextureSize;

        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateFloorTextures();

        // Assert
        Assert.Equal(expected, textures[ProceduralFloorCeilingTextures.FloorDarkBricks].Length);
    }

    [Fact]
    public void GenerateFloorTextures_DarkBricks_PixelsAreDark()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateFloorTextures();
        Color[] bricks = textures[ProceduralFloorCeilingTextures.FloorDarkBricks];

        // Assert — dark bricks should have low brightness (average R+G+B < 150)
        int totalBrightness = 0;
        for (int i = 0; i < bricks.Length; i++)
        {
            totalBrightness += bricks[i].R + bricks[i].G + bricks[i].B;
        }

        float avgBrightness = (float)totalBrightness / bricks.Length;
        Assert.True(avgBrightness < 150f, $"Expected dark texture, average brightness was {avgBrightness}");
    }

    [Fact]
    public void GenerateFloorTextures_DarkBricks_ContainsVariation()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateFloorTextures();
        Color[] bricks = textures[ProceduralFloorCeilingTextures.FloorDarkBricks];

        // Assert — not all pixels identical (has noise/mortar pattern)
        Color first = bricks[0];
        bool hasVariation = false;
        for (int i = 1; i < bricks.Length; i++)
        {
            if (bricks[i] != first)
            {
                hasVariation = true;
                break;
            }
        }

        Assert.True(hasVariation);
    }

    [Fact]
    public void GenerateFloorTextures_IsDeterministic()
    {
        // Act
        Color[][] first = ProceduralFloorCeilingTextures.GenerateFloorTextures();
        Color[][] second = ProceduralFloorCeilingTextures.GenerateFloorTextures();

        // Assert
        Assert.Equal(first[0], second[0]);
    }

    #endregion

    #region Ceiling Textures

    [Fact]
    public void GenerateCeilingTextures_ReturnsAtLeastTwoTextures()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();

        // Assert — space and dungeon
        Assert.True(textures.Length >= 2);
    }

    [Fact]
    public void GenerateCeilingTextures_Space_HasCorrectPixelCount()
    {
        // Arrange
        int expected = ProceduralFloorCeilingTextures.TextureSize * ProceduralFloorCeilingTextures.TextureSize;

        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();

        // Assert
        Assert.Equal(expected, textures[ProceduralFloorCeilingTextures.CeilingSpace].Length);
    }

    [Fact]
    public void GenerateCeilingTextures_Space_IsMostlyDark()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        Color[] space = textures[ProceduralFloorCeilingTextures.CeilingSpace];

        // Assert — space is very dark with occasional stars
        int darkPixels = 0;
        for (int i = 0; i < space.Length; i++)
        {
            if (space[i].R < 80 && space[i].G < 80 && space[i].B < 80)
                darkPixels++;
        }

        float darkRatio = (float)darkPixels / space.Length;
        Assert.True(darkRatio > 0.9f, $"Expected >90% dark pixels, got {darkRatio:P}");
    }

    [Fact]
    public void GenerateCeilingTextures_Space_ContainsBrightStars()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        Color[] space = textures[ProceduralFloorCeilingTextures.CeilingSpace];

        // Assert — some bright pixels exist (stars)
        bool hasBrightPixel = false;
        for (int i = 0; i < space.Length; i++)
        {
            if (space[i].R > 100 || space[i].G > 100 || space[i].B > 100)
            {
                hasBrightPixel = true;
                break;
            }
        }

        Assert.True(hasBrightPixel);
    }

    [Fact]
    public void GenerateCeilingTextures_Dungeon_HasCorrectPixelCount()
    {
        // Arrange
        int expected = ProceduralFloorCeilingTextures.TextureSize * ProceduralFloorCeilingTextures.TextureSize;

        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();

        // Assert
        Assert.Equal(expected, textures[ProceduralFloorCeilingTextures.CeilingDungeon].Length);
    }

    [Fact]
    public void GenerateCeilingTextures_Dungeon_ContainsVariation()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        Color[] dungeon = textures[ProceduralFloorCeilingTextures.CeilingDungeon];

        // Assert — has beams/cracks creating variation
        Color first = dungeon[0];
        bool hasVariation = false;
        for (int i = 1; i < dungeon.Length; i++)
        {
            if (dungeon[i] != first)
            {
                hasVariation = true;
                break;
            }
        }

        Assert.True(hasVariation);
    }

    [Fact]
    public void GenerateCeilingTextures_IsDeterministic()
    {
        // Act
        Color[][] first = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        Color[][] second = ProceduralFloorCeilingTextures.GenerateCeilingTextures();

        // Assert
        Assert.Equal(first[0], second[0]);
        Assert.Equal(first[1], second[1]);
    }

    #endregion

    #region Texture Size Constants

    [Fact]
    public void TextureSize_IsPowerOfTwo()
    {
        // Assert — required for bitwise masking in renderer
        int size = ProceduralFloorCeilingTextures.TextureSize;
        Assert.True((size & (size - 1)) == 0, $"TextureSize {size} is not a power of two");
    }

    [Fact]
    public void TextureSize_Is64()
    {
        Assert.Equal(64, ProceduralFloorCeilingTextures.TextureSize);
    }

    #endregion

    #region Pixel Alpha Validation

    [Fact]
    public void GenerateFloorTextures_DarkBricks_AllPixelsOpaque()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateFloorTextures();
        Color[] bricks = textures[ProceduralFloorCeilingTextures.FloorDarkBricks];

        // Assert — all pixels must have alpha = 255
        for (int i = 0; i < bricks.Length; i++)
        {
            Assert.Equal(255, bricks[i].A);
        }
    }

    [Fact]
    public void GenerateCeilingTextures_Space_AllPixelsOpaque()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        Color[] space = textures[ProceduralFloorCeilingTextures.CeilingSpace];

        // Assert
        for (int i = 0; i < space.Length; i++)
        {
            Assert.Equal(255, space[i].A);
        }
    }

    [Fact]
    public void GenerateCeilingTextures_Dungeon_AllPixelsOpaque()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        Color[] dungeon = textures[ProceduralFloorCeilingTextures.CeilingDungeon];

        // Assert
        for (int i = 0; i < dungeon.Length; i++)
        {
            Assert.Equal(255, dungeon[i].A);
        }
    }

    #endregion

    #region Star Count Validation

    [Fact]
    public void GenerateCeilingTextures_Space_StarCountWithinExpectedRange()
    {
        // Act
        Color[][] textures = ProceduralFloorCeilingTextures.GenerateCeilingTextures();
        Color[] space = textures[ProceduralFloorCeilingTextures.CeilingSpace];

        // Assert — bright stars (hash < 3) should be ~1.2% of pixels (≈49 of 4096)
        // Dim stars (hash < 8) add ~2% more. Total bright pixels (R>100) should be small but present.
        int brightCount = 0;
        for (int i = 0; i < space.Length; i++)
        {
            if (space[i].R > 100 || space[i].G > 100 || space[i].B > 100)
                brightCount++;
        }

        // Expect between 10 and 200 bright pixels (stars)
        Assert.InRange(brightCount, 10, 200);
    }

    #endregion
}
