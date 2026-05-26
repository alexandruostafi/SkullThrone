namespace SkullThrone;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Main game class for SkullThrone.
/// Manages the game loop, rendering at 320×200 logical resolution scaled to window.
/// </summary>
public sealed class SkullThroneGame : Game
{
    public const int LogicalWidth = 320;
    public const int LogicalHeight = 200;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private RenderTarget2D _renderTarget = null!;

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

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Draw game at logical resolution
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        // TODO: Raycaster rendering here

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
