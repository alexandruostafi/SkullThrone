namespace SkullThrone.Core.StateManagement;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Manages game state transitions with optional fade effects.
/// Owns all registered states and delegates Update/Draw to the active state.
/// </summary>
public sealed class GameStateManager
{
    private const float FadeDuration = 0.3f;

    private readonly Dictionary<GameStateId, IGameState> _states = new();
    private IGameState _currentState = null!;
    private GameStateId _currentId;

    private GameStateId _pendingStateId;
    private bool _isTransitioning;
    private float _fadeAlpha;
    private bool _fadingOut;
    private Texture2D _fadeTexture = null!;

    /// <summary>
    /// The currently active state identifier.
    /// </summary>
    public GameStateId CurrentStateId => _currentId;

    /// <summary>
    /// The previously active state identifier (before the current transition or last completed transition).
    /// </summary>
    public GameStateId PreviousStateId { get; private set; }

    /// <summary>
    /// Whether a fade transition is currently in progress.
    /// </summary>
    public bool IsTransitioning => _isTransitioning;

    /// <summary>
    /// Current fade alpha (0 = fully visible, 1 = fully black). Exposed for testing.
    /// </summary>
    public float FadeAlpha => _fadeAlpha;

    /// <summary>
    /// Initializes the fade texture. Must be called during LoadContent.
    /// </summary>
    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _fadeTexture = new Texture2D(graphicsDevice, 1, 1);
        _fadeTexture.SetData([Color.Black]);
    }

    /// <summary>
    /// Registers a state with the given identifier.
    /// </summary>
    public void RegisterState(GameStateId id, IGameState state)
    {
        _states[id] = state;
    }

    /// <summary>
    /// Sets the initial active state without a fade transition.
    /// </summary>
    public void SetInitialState(GameStateId id)
    {
        _currentId = id;
        _currentState = _states[id];
        _currentState.Enter();
    }

    /// <summary>
    /// Requests a transition to a new state with a fade effect.
    /// If already transitioning, the request is ignored.
    /// </summary>
    public void TransitionTo(GameStateId id)
    {
        if (_isTransitioning || id == _currentId)
            return;

        _pendingStateId = id;
        _isTransitioning = true;
        _fadingOut = true;
        _fadeAlpha = 0f;
    }

    /// <summary>
    /// Updates the active state and any ongoing transition.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_isTransitioning)
        {
            UpdateTransition(deltaTime);
        }
        else
        {
            _currentState.Update(gameTime);
        }
    }

    /// <summary>
    /// Draws the active state and the fade overlay if transitioning.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        _currentState.Draw(spriteBatch);

        if (_isTransitioning && _fadeAlpha > 0f)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            spriteBatch.Begin();
            spriteBatch.Draw(
                _fadeTexture,
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                Color.White * _fadeAlpha);
            spriteBatch.End();
        }
    }

    private void UpdateTransition(float deltaTime)
    {
        float fadeSpeed = 1f / FadeDuration;

        if (_fadingOut)
        {
            _fadeAlpha += fadeSpeed * deltaTime;
            if (_fadeAlpha >= 1f)
            {
                _fadeAlpha = 1f;
                _fadingOut = false;

                // Switch state at full black
                _currentState.Exit();
                PreviousStateId = _currentId;
                _currentId = _pendingStateId;
                _currentState = _states[_currentId];
                _currentState.Enter();
            }
        }
        else
        {
            _fadeAlpha -= fadeSpeed * deltaTime;
            if (_fadeAlpha <= 0f)
            {
                _fadeAlpha = 0f;
                _isTransitioning = false;
            }
        }
    }
}
