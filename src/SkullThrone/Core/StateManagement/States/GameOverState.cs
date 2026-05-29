namespace SkullThrone.Core.StateManagement.States;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Game Over state — stub for now. Shows "GAME OVER" text and a return to menu button.
/// Will be triggered by player death once health/combat systems are implemented.
/// </summary>
public sealed class GameOverState : IGameState
{
    private readonly GameContext _context;
    private UiButton _menuButton = null!;
    private MouseState _previousMouse;

    public GameOverState(GameContext context)
    {
        _context = context;
    }

    public void Enter()
    {
        _context.Game.IsMouseVisible = true;

        var font = _context.Font;
        float centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f;

        _menuButton = CreateCenteredButton("RETURN TO MENU", centerX, 300f, font);
        _previousMouse = Mouse.GetState();
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();

        if (_menuButton.Update(mouse, _previousMouse))
        {
            _context.StateManager.TransitionTo(GameStateId.Menu);
        }

        _previousMouse = mouse;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _context.Game.GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();

        var font = _context.Font;
        float centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f;

        string text = "GAME OVER";
        var textSize = font.MeasureString(text);
        spriteBatch.DrawString(font, text, new Vector2(centerX - textSize.X / 2f, 150f), Color.DarkRed);

        _menuButton.Draw(spriteBatch);

        spriteBatch.End();
    }

    private static UiButton CreateCenteredButton(string text, float centerX, float y, SpriteFont font)
    {
        var size = font.MeasureString(text);
        return new UiButton(text, new Vector2(centerX - size.X / 2f, y), font);
    }
}
