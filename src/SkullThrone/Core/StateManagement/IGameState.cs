namespace SkullThrone.Core.StateManagement;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a discrete game state (Menu, Playing, Paused, Options, GameOver).
/// Each state owns its own Update/Draw logic and lifecycle hooks.
/// </summary>
public interface IGameState
{
    /// <summary>
    /// Called when this state becomes the active state.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called when this state is being replaced by another state.
    /// </summary>
    void Exit();

    /// <summary>
    /// Update logic for this state. Called every frame while active.
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    /// Render logic for this state. Called every frame while active.
    /// </summary>
    void Draw(SpriteBatch spriteBatch);
}
