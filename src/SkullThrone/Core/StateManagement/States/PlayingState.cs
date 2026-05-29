namespace SkullThrone.Core.StateManagement.States;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkullThrone.Game.Entities;

/// <summary>
/// Active gameplay state — handles player movement, raycasting, and rendering the 3D view.
/// </summary>
public sealed class PlayingState : IGameState
{
    private const float MoveSpeed = 3f;
    private const float MouseSensitivity = 0.003f;
    private const float VerticalMouseSensitivity = 0.4f;

    private readonly GameContext _context;
    private KeyboardState _previousKeyState;
    private bool _skipMouseFrame;

    public PlayingState(GameContext context)
    {
        _context = context;
    }

    public void Enter()
    {
        _context.Game.IsMouseVisible = false;
        _skipMouseFrame = true;
        _previousKeyState = Keyboard.GetState();
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyState = Keyboard.GetState();
        var player = _context.Player;

        // Pause
        if (keyState.IsKeyDown(Keys.P) && _previousKeyState.IsKeyUp(Keys.P))
        {
            _context.StateManager.TransitionTo(GameStateId.Paused);
            _previousKeyState = keyState;
            return;
        }

        // F11 fullscreen toggle
        if (keyState.IsKeyDown(Keys.F11) && _previousKeyState.IsKeyUp(Keys.F11))
        {
            _context.Graphics.IsFullScreen = !_context.Graphics.IsFullScreen;
            _context.Graphics.ApplyChanges();
            _skipMouseFrame = true;
        }

        // Mouse look
        HandleMouseLook(player);

        // Movement
        HandleMovement(keyState, player, deltaTime);

        // Portal cooldown
        _context.PortalTeleporter.Update(deltaTime);

        // Cast rays
        _context.Raycaster.CastAllRays(
            player.X, player.Y, player.Angle,
            _context.Map.Tiles, _context.Map.Width, _context.Map.Height);

        _previousKeyState = keyState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _context.Game.GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        var destRect = _context.GetLetterboxRect();
        var player = _context.Player;
        _context.WallRenderer.Draw(
            spriteBatch, _context.Raycaster.HitBuffer, destRect,
            player.X, player.Y, player.Angle, player.Pitch, _context.Map);
        spriteBatch.End();
    }

    private void HandleMouseLook(Player player)
    {
        var mouseState = Mouse.GetState();
        int centerX = _context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2;
        int centerY = _context.Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2;

        if (_skipMouseFrame)
        {
            _skipMouseFrame = false;
        }
        else
        {
            int mouseDeltaX = mouseState.X - centerX;
            int mouseDeltaY = mouseState.Y - centerY;
            player.Angle += mouseDeltaX * MouseSensitivity;
            player.Pitch -= (int)(mouseDeltaY * VerticalMouseSensitivity);
        }

        Mouse.SetPosition(centerX, centerY);
    }

    private void HandleMovement(KeyboardState keyState, Player player, float deltaTime)
    {
        float moveX = 0f;
        float moveY = 0f;
        float dirX = MathF.Cos(player.Angle);
        float dirY = MathF.Sin(player.Angle);

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

        // Portal teleportation check
        if (!_context.PortalTeleporter.TryTeleport(player, moveX, moveY, _context.Map))
        {
            // Wall sliding collision
            if (_context.Map.GetTile((int)(player.X + moveX), (int)player.Y) == 0)
                player.X += moveX;
            if (_context.Map.GetTile((int)player.X, (int)(player.Y + moveY)) == 0)
                player.Y += moveY;
        }
    }
}
