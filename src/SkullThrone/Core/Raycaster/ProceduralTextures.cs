namespace SkullThrone.Core.Raycaster;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Generates procedural wall textures at runtime for development/placeholder purposes.
/// All textures are 64×128 pixels.
/// </summary>
public static class ProceduralTextures
{
    public const int TextureWidth = 64;
    public const int TextureHeight = 128;

    /// <summary>
    /// Generates all placeholder wall textures and returns them indexed by texture ID.
    /// Index 0 is null (no wall).
    /// </summary>
    public static Texture2D?[] GenerateAll(GraphicsDevice graphicsDevice)
    {
        return
        [
            null,                                        // 0: empty
            GenerateBrick(graphicsDevice),               // 1: blood-stained stone
            GenerateMetal(graphicsDevice),               // 2: metal panels
            GenerateBrass(graphicsDevice),               // 3: brass/khorne trim
            GenerateDarkCorridor(graphicsDevice)         // 4: dark corridor stone
        ];
    }

    private static Texture2D GenerateBrick(GraphicsDevice graphicsDevice)
    {
        var pixels = new Color[TextureWidth * TextureHeight];
        var baseColor = new Color(120, 30, 30);
        var mortarColor = new Color(50, 20, 20);

        for (int y = 0; y < TextureHeight; y++)
        {
            for (int x = 0; x < TextureWidth; x++)
            {
                bool isMortarY = y % 16 < 2;
                int offset = (y / 16 % 2 == 0) ? 0 : 32;
                bool isMortarX = (x + offset) % 32 < 2;

                if (isMortarY || isMortarX)
                {
                    pixels[y * TextureWidth + x] = mortarColor;
                }
                else
                {
                    // Add noise variation
                    int noise = ((x * 7 + y * 13) % 20) - 10;
                    pixels[y * TextureWidth + x] = new Color(
                        (byte)Math.Clamp(baseColor.R + noise, 0, 255),
                        (byte)Math.Clamp(baseColor.G + noise / 3, 0, 255),
                        (byte)Math.Clamp(baseColor.B + noise / 3, 0, 255));
                }
            }
        }

        return CreateTexture(graphicsDevice, pixels);
    }

    private static Texture2D GenerateMetal(GraphicsDevice graphicsDevice)
    {
        var pixels = new Color[TextureWidth * TextureHeight];
        var baseColor = new Color(100, 105, 110);
        var rivetColor = new Color(140, 145, 150);
        var seamColor = new Color(50, 52, 55);

        for (int y = 0; y < TextureHeight; y++)
        {
            for (int x = 0; x < TextureWidth; x++)
            {
                // Horizontal seams every 32 pixels
                if (y % 32 == 0)
                {
                    pixels[y * TextureWidth + x] = seamColor;
                }
                // Rivets at corners of panels
                else if ((x == 4 || x == 60) && (y % 32 == 4 || y % 32 == 28))
                {
                    pixels[y * TextureWidth + x] = rivetColor;
                }
                else
                {
                    // Subtle vertical streaks
                    int streak = (x * 3 + y * 7) % 15 - 7;
                    pixels[y * TextureWidth + x] = new Color(
                        (byte)Math.Clamp(baseColor.R + streak, 0, 255),
                        (byte)Math.Clamp(baseColor.G + streak, 0, 255),
                        (byte)Math.Clamp(baseColor.B + streak, 0, 255));
                }
            }
        }

        return CreateTexture(graphicsDevice, pixels);
    }

    private static Texture2D GenerateBrass(GraphicsDevice graphicsDevice)
    {
        var pixels = new Color[TextureWidth * TextureHeight];
        var baseColor = new Color(160, 120, 40);
        var trimColor = new Color(200, 160, 60);
        var darkColor = new Color(80, 60, 20);

        for (int y = 0; y < TextureHeight; y++)
        {
            for (int x = 0; x < TextureWidth; x++)
            {
                // Decorative border trim
                bool isBorder = x < 4 || x >= 60 || y % 64 < 4 || y % 64 >= 60;

                if (isBorder)
                {
                    pixels[y * TextureWidth + x] = trimColor;
                }
                // Skull-like circular motif in center of each 64×64 panel
                else
                {
                    int cx = x - 32;
                    int cy = (y % 64) - 32;
                    int dist = cx * cx + cy * cy;

                    if (dist < 100)
                    {
                        pixels[y * TextureWidth + x] = darkColor;
                    }
                    else
                    {
                        int noise = ((x * 11 + y * 5) % 16) - 8;
                        pixels[y * TextureWidth + x] = new Color(
                            (byte)Math.Clamp(baseColor.R + noise, 0, 255),
                            (byte)Math.Clamp(baseColor.G + noise, 0, 255),
                            (byte)Math.Clamp(baseColor.B + noise / 2, 0, 255));
                    }
                }
            }
        }

        return CreateTexture(graphicsDevice, pixels);
    }

    private static Texture2D GenerateDarkCorridor(GraphicsDevice graphicsDevice)
    {
        var pixels = new Color[TextureWidth * TextureHeight];
        var baseColor = new Color(40, 45, 50);
        var crackColor = new Color(20, 22, 25);

        for (int y = 0; y < TextureHeight; y++)
        {
            for (int x = 0; x < TextureWidth; x++)
            {
                // Random-looking cracks using hash
                int hash = (x * 31 + y * 17) % 97;
                if (hash < 5)
                {
                    pixels[y * TextureWidth + x] = crackColor;
                }
                else
                {
                    int noise = ((x * 13 + y * 7) % 12) - 6;
                    pixels[y * TextureWidth + x] = new Color(
                        (byte)Math.Clamp(baseColor.R + noise, 0, 255),
                        (byte)Math.Clamp(baseColor.G + noise, 0, 255),
                        (byte)Math.Clamp(baseColor.B + noise, 0, 255));
                }
            }
        }

        return CreateTexture(graphicsDevice, pixels);
    }

    private static Texture2D CreateTexture(GraphicsDevice graphicsDevice, Color[] pixels)
    {
        var texture = new Texture2D(graphicsDevice, TextureWidth, TextureHeight);
        texture.SetData(pixels);
        return texture;
    }
}
