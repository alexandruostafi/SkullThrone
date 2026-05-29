namespace SkullThrone.Core.StateManagement;

/// <summary>
/// Identifies the available game states for transition requests.
/// </summary>
public enum GameStateId
{
    Menu,
    Playing,
    Paused,
    Options,
    GameOver
}
