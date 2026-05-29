namespace SkullThrone.Core.StateManagement;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// A simple text-based UI button that detects mouse hover and click.
/// Pre-allocated — no allocations on Update/Draw.
/// </summary>
public sealed class UiButton
{
    private readonly string _text;
    private readonly Vector2 _position;
    private readonly SpriteFont _font;
    private readonly Vector2 _textSize;
    private Rectangle _bounds;
    private bool _isHovered;

    public UiButton(string text, Vector2 position, SpriteFont font)
    {
        _text = text;
        _position = position;
        _font = font;
        _textSize = font.MeasureString(text);
        _bounds = new Rectangle(
            (int)position.X,
            (int)position.Y,
            (int)_textSize.X,
            (int)_textSize.Y);
    }

    /// <summary>
    /// Returns true the frame the button is clicked (left mouse released over button).
    /// </summary>
    public bool Update(MouseState currentMouse, MouseState previousMouse)
    {
        _isHovered = _bounds.Contains(currentMouse.X, currentMouse.Y);
        bool clicked = _isHovered
            && currentMouse.LeftButton == ButtonState.Released
            && previousMouse.LeftButton == ButtonState.Pressed;
        return clicked;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var color = _isHovered ? Color.Red : Color.White;
        spriteBatch.DrawString(_font, _text, _position, color);
    }
}
