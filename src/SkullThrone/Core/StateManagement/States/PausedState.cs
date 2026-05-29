namespace SkullThrone.Core.StateManagement.States;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Paused state — displays "PAUSED" text with Options and Exit to Menu buttons.
/// The game view is frozen underneath (drawn by the playing state's last frame).
/// </summary>
public sealed class PausedState : IGameState
{
    private readonly GameContext _context;
    private UiButton _optionsButton = null!;
    private UiButton _exitToMenuButton = null!;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyState;
    private Texture2D _overlayTexture = null!;

    public PausedState(GameContext context)
    {
        _context = context;
    }

    public void Enter()
    {
        _context.Game.IsMouseVisible = true;

        if (_overlayTexture is null)
        {
            _overlayTexture = new Texture2D(_context.Game.GraphicsDevice, 1, 1);
            _overlayTexture.SetData([Color.Black]);
        }

        var font = _context.Font;
        float centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f;
        float startY = 260f;
        float spacing = 50f;

        _optionsButton = CreateCenteredButton("OPTIONS", centerX, startY, font);
        _exitToMenuButton = CreateCenteredButton("EXIT TO MENU", centerX, startY + spacing, font);

        _previousMouse = Mouse.GetState();
        _previousKeyState = Keyboard.GetState();
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        var keyState = Keyboard.GetState();
        var mouse = Mouse.GetState();

        // Resume on P
        if (keyState.IsKeyDown(Keys.P) && _previousKeyState.IsKeyUp(Keys.P))
        {
            _context.StateManager.TransitionTo(GameStateId.Playing);
            _previousKeyState = keyState;
            return;
        }

        if (_optionsButton.Update(mouse, _previousMouse))
        {
            _context.StateManager.TransitionTo(GameStateId.Options);
        }
        else if (_exitToMenuButton.Update(mouse, _previousMouse))
        {
            _context.StateManager.TransitionTo(GameStateId.Menu);
        }

        _previousMouse = mouse;
        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw the frozen game view underneath
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        var destRect = _context.GetLetterboxRect();
        var player = _context.Player;
        _context.WallRenderer.Draw(
            spriteBatch, _context.Raycaster.HitBuffer, destRect,
            player.X, player.Y, player.Angle, player.Pitch, _context.Map);
        spriteBatch.End();

        // Dark overlay
        var viewport = _context.Game.GraphicsDevice.Viewport;
        spriteBatch.Begin();

        // Semi-transparent overlay
        spriteBatch.Draw(_overlayTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White * 0.6f);

        // "PAUSED" title
        var font = _context.Font;
        string pausedText = "PAUSED";
        var textSize = font.MeasureString(pausedText);
        float centerX = viewport.Width / 2f;
        spriteBatch.DrawString(font, pausedText, new Vector2(centerX - textSize.X / 2f, 150f), Color.White);

        _optionsButton.Draw(spriteBatch);
        _exitToMenuButton.Draw(spriteBatch);

        spriteBatch.End();
    }

    private static UiButton CreateCenteredButton(string text, float centerX, float y, SpriteFont font)
    {
        var size = font.MeasureString(text);
        return new UiButton(text, new Vector2(centerX - size.X / 2f, y), font);
    }
}
