using Weave.InputSources;
using Weave.Networking;

namespace Weave;

/// <summary>
///     Game configuration.
///     Only use static fields to pass data that NEEDS to be transferred between scenes.
/// </summary>
public static class GameConfig
{
    /// <summary>
    ///     The lobby.
    /// </summary>
    public static Lobby Lobby { get; set; }

    public static RTCClientManager MultiplayerManager { get; set; }

    public static bool HasLocks(int nPlayers)
    {
        return nPlayers != 1;
    }

    /// <summary>
    ///     Gets the initial movement speed based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The initial movement speed.</returns>
    public static float GetInitialMovementSpeed(int nPlayers)
    {
        return nPlayers switch
        {
            <= 2 => 100,
            3 => 75,
            _ => 50
        };
    }

    /// <summary>
    ///     Gets the number of obstacles based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The number of obstacles.</returns>
    public static int GetNObstacles(int nPlayers)
    {
        return nPlayers switch
        {
            1 => 6,
            2 => 5,
            3 => 4,
            _ => 0
        };
    }

    /// <summary>
    ///     Gets the acceleration based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The acceleration.</returns>
    public static float GetAcceleration(int nPlayers)
    {
        return nPlayers switch
        {
            1 => 6.66f,
            2 => 2.1f,
            3 => 2.05f,
            4 => 2.0f,
            _ => 1.8f
        };
    }

    /// <summary>
    ///     Gets the turn speed acceleration based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The turn speed acceleration.</returns>
    public static float GetTurnSpeedAcceleration(int nPlayers)
    {
        return nPlayers switch
        {
            <= 2 => 1.5f,
            _ => 1.25f
        };
    }
}
