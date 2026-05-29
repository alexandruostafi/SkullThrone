namespace SkullThrone;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkullThrone.Core.Physics;
using SkullThrone.Core.Raycaster;
using SkullThrone.Core.StateManagement;
using SkullThrone.Core.StateManagement.States;
using SkullThrone.Game.Entities;
using SkullThrone.Game.Levels;
using XnaGame = Microsoft.Xna.Framework.Game;

/// <summary>
/// Main game class for SkullThrone.
/// Manages the game loop, rendering at 320×200 logical resolution scaled to window.
/// Delegates all state-specific logic to the <see cref="GameStateManager"/>.
/// </summary>
public sealed class SkullThroneGame : XnaGame
{
    public const int LogicalWidth = 320;
    public const int LogicalHeight = 200;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private GameStateManager _stateManager = null!;

    public SkullThroneGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            HardwareModeSwitch = false
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = LogicalWidth * 3;
        _graphics.PreferredBackBufferHeight = LogicalHeight * 3;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var font = Content.Load<SpriteFont>("Fonts/MenuFont");
        var raycaster = new DdaRaycaster();
        var map = MapData.CreateTestMap();
        var player = new Player(4f, 8f, 0f);
        var portalTeleporter = new PortalTeleporter();
        var wallRenderer = new WallRenderer(GraphicsDevice);

        _stateManager = new GameStateManager();
        _stateManager.LoadContent(GraphicsDevice);

        var context = new GameContext
        {
            Game = this,
            Graphics = _graphics,
            StateManager = _stateManager,
            SpriteBatch = _spriteBatch,
            Raycaster = raycaster,
            WallRenderer = wallRenderer,
            Map = map,
            Player = player,
            PortalTeleporter = portalTeleporter,
            Font = font
        };

        var optionsState = new OptionsState(context);

        _stateManager.RegisterState(GameStateId.Menu, new MenuState(context));
        _stateManager.RegisterState(GameStateId.Playing, new PlayingState(context));
        _stateManager.RegisterState(GameStateId.Paused, new PausedState(context));
        _stateManager.RegisterState(GameStateId.Options, optionsState);
        _stateManager.RegisterState(GameStateId.GameOver, new GameOverState(context));

        _stateManager.SetInitialState(GameStateId.Menu);
    }

    protected override void Update(GameTime gameTime)
    {
        _stateManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _stateManager.Draw(_spriteBatch);
        base.Draw(gameTime);
    }

    /// <summary>
    /// Calculates the destination rectangle that preserves the logical aspect ratio
    /// within the given window dimensions, centering with letterbox/pillarbox margins.
    /// </summary>
    internal static Rectangle CalculateLetterboxRect(int windowWidth, int windowHeight)
    {
        var scale = Math.Min(
            (float)windowWidth / LogicalWidth,
            (float)windowHeight / LogicalHeight);

        var width = (int)(LogicalWidth * scale);
        var height = (int)(LogicalHeight * scale);
        var x = (windowWidth - width) / 2;
        var y = (windowHeight - height) / 2;

        return new Rectangle(x, y, width, height);
    }
}
