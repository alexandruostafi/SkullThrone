namespace SkullThrone.Core.StateManagement.States;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Options state — placeholder for future settings (resolution, controls, etc.).
/// Accessible from both Menu and Paused states. Shows a Back button to return.
/// </summary>
public sealed class OptionsState : IGameState
{
    private readonly GameContext _context;
    private UiButton _backButton = null!;
    private MouseState _previousMouse;

    public OptionsState(GameContext context)
    {
        _context = context;
    }

    public void Enter()
    {
        _context.Game.IsMouseVisible = true;

        var font = _context.Font;
        float centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f;

        _backButton = CreateCenteredButton("BACK", centerX, 350f, font);
        _previousMouse = Mouse.GetState();
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();

        if (_backButton.Update(mouse, _previousMouse))
        {
            // Return to whichever state opened Options (Menu or Paused)
            _context.StateManager.TransitionTo(_context.StateManager.PreviousStateId);
        }

        _previousMouse = mouse;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _context.Game.GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();

        var font = _context.Font;
        float centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f;

        string title = "OPTIONS";
        var titleSize = font.MeasureString(title);
        spriteBatch.DrawString(font, title, new Vector2(centerX - titleSize.X / 2f, 80f), Color.White);

        string placeholder = "(No options available yet)";
        var phSize = font.MeasureString(placeholder);
        spriteBatch.DrawString(font, placeholder, new Vector2(centerX - phSize.X / 2f, 200f), Color.Gray);

        _backButton.Draw(spriteBatch);

        spriteBatch.End();
    }

    private static UiButton CreateCenteredButton(string text, float centerX, float y, SpriteFont font)
    {
        var size = font.MeasureString(text);
        return new UiButton(text, new Vector2(centerX - size.X / 2f, y), font);
    }
}
