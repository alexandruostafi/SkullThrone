namespace SkullThrone.Core.StateManagement.States;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Main menu state displaying title, Start, Options, and Exit buttons.
/// </summary>
public sealed class MenuState : IGameState
{
    private readonly GameContext _context;
    private UiButton _startButton = null!;
    private UiButton _optionsButton = null!;
    private UiButton _exitButton = null!;
    private MouseState _previousMouse;

    public MenuState(GameContext context)
    {
        _context = context;
    }

    public void Enter()
    {
        _context.Game.IsMouseVisible = true;

        var font = _context.Font;
        float centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f;
        float startY = 200f;
        float spacing = 50f;

        _startButton = CreateCenteredButton("START", centerX, startY, font);
        _optionsButton = CreateCenteredButton("OPTIONS", centerX, startY + spacing, font);
        _exitButton = CreateCenteredButton("EXIT", centerX, startY + spacing * 2, font);

        _previousMouse = Mouse.GetState();
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();

        if (_startButton.Update(mouse, _previousMouse))
        {
            _context.StateManager.TransitionTo(GameStateId.Playing);
        }
        else if (_optionsButton.Update(mouse, _previousMouse))
        {
            _context.StateManager.TransitionTo(GameStateId.Options);
        }
        else if (_exitButton.Update(mouse, _previousMouse))
        {
            _context.Game.Exit();
        }

        _previousMouse = mouse;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _context.Game.GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();

        // Title
        var font = _context.Font;
        string title = "SKULLTHRONE";
        var titleSize = font.MeasureString(title);
        float centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f;
        spriteBatch.DrawString(font, title, new Vector2(centerX - titleSize.X / 2f, 80f), Color.DarkRed);

        _startButton.Draw(spriteBatch);
        _optionsButton.Draw(spriteBatch);
        _exitButton.Draw(spriteBatch);

        spriteBatch.End();
    }

    private static UiButton CreateCenteredButton(string text, float centerX, float y, SpriteFont font)
    {
        var size = font.MeasureString(text);
        return new UiButton(text, new Vector2(centerX - size.X / 2f, y), font);
    }
}
