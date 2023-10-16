using Weave.InputSources;

namespace Weave;

/// <summary>
///     Game configuration.
///     Only use static fields to pass data that NEEDS to be transferred between scenes.
/// </summary>
public static class GameConfig
{
    /// <summary>
    /// The lobby.
    /// </summary>
    public static Lobby Lobby { get; set; }

    /// <summary>
    /// Determines whether the game should have locks based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>True if the game should have locks, false otherwise.</returns>
    public static bool ShouldHaveLocks(int nPlayers) => nPlayers <= 2;

    /// <summary>
    /// Gets the initial movement speed based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The initial movement speed.</returns>
    public static float GetInitialMovementSpeed(int nPlayers)
    {
        return nPlayers switch
        {
            <= 2 => 130,
            3 => 100,
            _ => 70
        };
    }

    /// <summary>
    /// Gets the number of obstacles based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The number of obstacles.</returns>
    public static int GetNObstacles(int nPlayers)
    {
        return nPlayers switch
        {
            <= 2 => 4,
            3 => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the acceleration based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The acceleration.</returns>
    public static float GetAcceleration(int nPlayers)
    {
        if (nPlayers <= 2)
            return 1.4f;

        return 1.1f;
    }

    /// <summary>
    /// Gets the turn speed acceleration based on the number of players.
    /// </summary>
    /// <param name="nPlayers">The number of players.</param>
    /// <returns>The turn speed acceleration.</returns>
    public static float GetTurnSpeedAcceleration(int nPlayers)
    {
        if (nPlayers <= 2)
            return 1.5f;

        return 1.25f;
    }
}
