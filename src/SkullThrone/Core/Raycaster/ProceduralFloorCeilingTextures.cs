namespace SkullThrone.Core.Raycaster;

using Microsoft.Xna.Framework;

/// <summary>
/// Generates procedural floor and ceiling textures at runtime.
/// All textures are 64×64 pixels (square for floor/ceiling casting).
/// </summary>
public static class ProceduralFloorCeilingTextures
{
    public const int TextureSize = 64;

    /// <summary>Floor texture ID for dark bricks.</summary>
    public const int FloorDarkBricks = 0;

    /// <summary>Ceiling texture ID for space/void.</summary>
    public const int CeilingSpace = 0;

    /// <summary>Ceiling texture ID for indoor dungeon.</summary>
    public const int CeilingDungeon = 1;

    /// <summary>
    /// Generates all floor textures. Returns pixel arrays indexed by texture ID.
    /// </summary>
    public static Color[][] GenerateFloorTextures()
    {
        return
        [
            GenerateDarkBricks()
        ];
    }

    /// <summary>
    /// Generates all ceiling textures. Returns pixel arrays indexed by texture ID.
    /// </summary>
    public static Color[][] GenerateCeilingTextures()
    {
        return
        [
            GenerateSpace(),
            GenerateDungeon()
        ];
    }

    /// <summary>
    /// Dark blackish brick texture for floors.
    /// </summary>
    private static Color[] GenerateDarkBricks()
    {
        var pixels = new Color[TextureSize * TextureSize];
        var baseColor = new Color(25, 22, 20);
        var mortarColor = new Color(12, 10, 10);
        var highlightColor = new Color(35, 30, 28);

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                // Brick pattern: 16x8 bricks with offset rows
                int brickH = 8;
                int brickW = 16;
                int offset = (y / brickH % 2 == 0) ? 0 : brickW / 2;
                bool isMortarY = y % brickH < 1;
                bool isMortarX = (x + offset) % brickW < 1;

                if (isMortarY || isMortarX)
                {
                    pixels[y * TextureSize + x] = mortarColor;
                }
                else
                {
                    int noise = ((x * 7 + y * 13) % 16) - 8;
                    // Occasional lighter bricks
                    Color brick = ((x / brickW + y / brickH) % 5 == 0) ? highlightColor : baseColor;
                    pixels[y * TextureSize + x] = new Color(
                        (byte)Math.Clamp(brick.R + noise, 0, 255),
                        (byte)Math.Clamp(brick.G + noise, 0, 255),
                        (byte)Math.Clamp(brick.B + noise, 0, 255));
                }
            }
        }

        return pixels;
    }

    /// <summary>
    /// Space/void texture for outdoor ceilings — dark with scattered stars.
    /// </summary>
    private static Color[] GenerateSpace()
    {
        var pixels = new Color[TextureSize * TextureSize];
        var baseColor = new Color(5, 3, 12);
        var nebulaColor = new Color(15, 5, 20);

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                // Hash for pseudo-random stars
                int hash = (x * 131 + y * 97) % 251;

                if (hash < 3)
                {
                    // Bright stars
                    int brightness = 150 + (hash * 35);
                    pixels[y * TextureSize + x] = new Color(brightness, brightness, brightness + 20);
                }
                else if (hash < 8)
                {
                    // Dim stars
                    pixels[y * TextureSize + x] = new Color(60, 55, 70);
                }
                else
                {
                    // Dark background with subtle nebula variation
                    int nebula = ((x * 3 + y * 7) % 19);
                    Color bg = nebula < 4 ? nebulaColor : baseColor;
                    int noise = ((x * 11 + y * 23) % 8) - 4;
                    pixels[y * TextureSize + x] = new Color(
                        (byte)Math.Clamp(bg.R + noise, 0, 255),
                        (byte)Math.Clamp(bg.G + noise, 0, 255),
                        (byte)Math.Clamp(bg.B + noise, 0, 255));
                }
            }
        }

        return pixels;
    }

    /// <summary>
    /// Indoor dungeon ceiling — dark stone with cracks and support beams.
    /// </summary>
    private static Color[] GenerateDungeon()
    {
        var pixels = new Color[TextureSize * TextureSize];
        var baseColor = new Color(35, 30, 32);
        var beamColor = new Color(50, 40, 35);
        var crackColor = new Color(15, 12, 14);

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                // Support beams crossing at grid intervals
                bool isBeam = (x % 32 < 3) || (y % 32 < 3);

                if (isBeam)
                {
                    int noise = ((x * 5 + y * 3) % 10) - 5;
                    pixels[y * TextureSize + x] = new Color(
                        (byte)Math.Clamp(beamColor.R + noise, 0, 255),
                        (byte)Math.Clamp(beamColor.G + noise, 0, 255),
                        (byte)Math.Clamp(beamColor.B + noise, 0, 255));
                }
                else
                {
                    // Cracks via hash
                    int hash = (x * 31 + y * 17) % 89;
                    if (hash < 4)
                    {
                        pixels[y * TextureSize + x] = crackColor;
                    }
                    else
                    {
                        int noise = ((x * 9 + y * 11) % 14) - 7;
                        pixels[y * TextureSize + x] = new Color(
                            (byte)Math.Clamp(baseColor.R + noise, 0, 255),
                            (byte)Math.Clamp(baseColor.G + noise, 0, 255),
                            (byte)Math.Clamp(baseColor.B + noise, 0, 255));
                    }
                }
            }
        }

        return pixels;
    }
}
