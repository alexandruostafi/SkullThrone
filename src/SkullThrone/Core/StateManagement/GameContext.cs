namespace SkullThrone.Core.StateManagement;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkullThrone.Core.Physics;
using SkullThrone.Core.Raycaster;
using SkullThrone.Game.Entities;
using SkullThrone.Game.Levels;
using XnaGame = Microsoft.Xna.Framework.Game;

/// <summary>
/// Shared context passed to all game states, providing access to core systems and resources.
/// Avoids tight coupling between states and the main game class.
/// </summary>
public sealed class GameContext
{
    public required XnaGame Game { get; init; }
    public required GraphicsDeviceManager Graphics { get; init; }
    public required GameStateManager StateManager { get; init; }
    public required SpriteBatch SpriteBatch { get; init; }
    public required DdaRaycaster Raycaster { get; init; }
    public required WallRenderer WallRenderer { get; init; }
    public required MapData Map { get; init; }
    public required Player Player { get; init; }
    public required PortalTeleporter PortalTeleporter { get; init; }
    public required SpriteFont Font { get; init; }

    /// <summary>
    /// Calculates the letterboxed destination rectangle for the current window size.
    /// </summary>
    public Rectangle GetLetterboxRect()
    {
        int windowWidth = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
        int windowHeight = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;
        return SkullThroneGame.CalculateLetterboxRect(windowWidth, windowHeight);
    }
}
