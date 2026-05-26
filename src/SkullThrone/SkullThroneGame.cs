namespace SkullThrone;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    private const float RotateSpeed = 2.5f;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private RenderTarget2D _renderTarget = null!;

    private DdaRaycaster _raycaster = null!;
    private WallRenderer _wallRenderer = null!;
    private MapData _map = null!;
    private Player _player = null!;

    public SkullThroneGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = LogicalWidth * 3;
        _graphics.PreferredBackBufferHeight = LogicalHeight * 3;
        _graphics.ApplyChanges();

        _raycaster = new DdaRaycaster();
        _map = MapData.CreateTestMap();
        _player = new Player(8f, 8f, 0f);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);
        _wallRenderer = new WallRenderer(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyState = Keyboard.GetState();

        if (keyState.IsKeyDown(Keys.Escape))
            Exit();

        // Rotation
        if (keyState.IsKeyDown(Keys.Left))
            _player.Angle -= RotateSpeed * deltaTime;
        if (keyState.IsKeyDown(Keys.Right))
            _player.Angle += RotateSpeed * deltaTime;

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

        // Simple collision: check X and Y separately for wall sliding
        if (_map.GetTile((int)(_player.X + moveX), (int)_player.Y) == 0)
            _player.X += moveX;
        if (_map.GetTile((int)_player.X, (int)(_player.Y + moveY)) == 0)
            _player.Y += moveY;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Cast rays
        _raycaster.CastAllRays(_player.X, _player.Y, _player.Angle, _map.Tiles, _map.Width, _map.Height);

        // Draw game at logical resolution
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _wallRenderer.DrawFloorCeiling(_spriteBatch);
        _wallRenderer.Draw(_spriteBatch, _raycaster.HitBuffer);
        _spriteBatch.End();

        // Scale to window
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        var destinationRect = CalculateLetterboxRect();
        _spriteBatch.Draw(_renderTarget, destinationRect, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Rectangle CalculateLetterboxRect()
    {
        var windowWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        var windowHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

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
