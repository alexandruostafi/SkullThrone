namespace SkullThrone;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkullThrone.Core.Physics;
using SkullThrone.Core.Raycaster;
using SkullThrone.Game.Entities;
using SkullThrone.Game.Levels;
using XnaGame = Microsoft.Xna.Framework.Game;

/// <summary>
/// Main game class for SkullThrone.
/// Manages the game loop, rendering at 320×200 logical resolution scaled to window.
/// </summary>
public sealed class SkullThroneGame : XnaGame
{
    public const int LogicalWidth = 320;
    public const int LogicalHeight = 200;

    private const float MoveSpeed = 3f;
    private const float MouseSensitivity = 0.003f;
    private const float VerticalMouseSensitivity = 0.4f;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private KeyboardState _previousKeyState;
    private bool _skipMouseFrame;

    private DdaRaycaster _raycaster = null!;
    private WallRenderer _wallRenderer = null!;
    private MapData _map = null!;
    private Player _player = null!;
    private PortalTeleporter _portalTeleporter = null!;

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

        _raycaster = new DdaRaycaster();
        _map = MapData.CreateTestMap();
        _player = new Player(4f, 8f, 0f);
        _portalTeleporter = new PortalTeleporter();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _wallRenderer = new WallRenderer(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyState = Keyboard.GetState();

        if (keyState.IsKeyDown(Keys.Escape))
            Exit();

        // F11 toggles borderless fullscreen
        if (keyState.IsKeyDown(Keys.F11) && _previousKeyState.IsKeyUp(Keys.F11))
            ToggleFullscreen();

        // Mouse look
        var mouseState = Mouse.GetState();
        int centerX = GraphicsDevice.PresentationParameters.BackBufferWidth / 2;
        int centerY = GraphicsDevice.PresentationParameters.BackBufferHeight / 2;

        if (_skipMouseFrame)
        {
            _skipMouseFrame = false;
        }
        else
        {
            int mouseDeltaX = mouseState.X - centerX;
            int mouseDeltaY = mouseState.Y - centerY;
            _player.Angle += mouseDeltaX * MouseSensitivity;
            _player.Pitch -= (int)(mouseDeltaY * VerticalMouseSensitivity);
        }

        Mouse.SetPosition(centerX, centerY);

        // Movement
        float moveX = 0f;
        float moveY = 0f;
        float dirX = MathF.Cos(_player.Angle);
        float dirY = MathF.Sin(_player.Angle);

        if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up))
        {
            moveX += dirX * MoveSpeed * deltaTime;
            moveY += dirY * MoveSpeed * deltaTime;
        }
        if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down))
        {
            moveX -= dirX * MoveSpeed * deltaTime;
            moveY -= dirY * MoveSpeed * deltaTime;
        }
        if (keyState.IsKeyDown(Keys.A))
        {
            moveX += dirY * MoveSpeed * deltaTime;
            moveY -= dirX * MoveSpeed * deltaTime;
        }
        if (keyState.IsKeyDown(Keys.D))
        {
            moveX -= dirY * MoveSpeed * deltaTime;
            moveY += dirX * MoveSpeed * deltaTime;
        }

        // Portal cooldown tick (always, even when player is stationary)
        _portalTeleporter.Update(deltaTime);

        // Portal teleportation check (before standard collision)
        if (!_portalTeleporter.TryTeleport(_player, moveX, moveY, _map))
        {
            // Simple collision: check X and Y separately for wall sliding
            if (_map.GetTile((int)(_player.X + moveX), (int)_player.Y) == 0)
                _player.X += moveX;
            if (_map.GetTile((int)_player.X, (int)(_player.Y + moveY)) == 0)
                _player.Y += moveY;
        }

        // Cast rays (computation, not rendering)
        _raycaster.CastAllRays(_player.X, _player.Y, _player.Angle, _map.Tiles, _map.Width, _map.Height);

        _previousKeyState = keyState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        var destinationRect = CalculateLetterboxRect();
        _wallRenderer.Draw(_spriteBatch, _raycaster.HitBuffer, destinationRect, _player.X, _player.Y, _player.Angle, _player.Pitch, _map);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void ToggleFullscreen()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();
        _skipMouseFrame = true;
    }

    private Rectangle CalculateLetterboxRect()
    {
        var windowWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        var windowHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

        return CalculateLetterboxRect(windowWidth, windowHeight);
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
