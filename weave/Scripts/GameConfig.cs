using Weave.InputSources;

namespace Weave;

/// <summary>
///     Game configuration.
///     Only use static fields to pass data that NEEDS to be transferred between scenes.
/// </summary>
public static class GameConfig
{
    public static Lobby Lobby { get; set; }
    public static Multiplayer.Manager MultiplayerManager { get; set; }

    public static float GetInitialPlayerMovement(int nPlayers)
    {
        return nPlayers switch
        {
            <= 2 => 130,
            3 => 100,
            _ => 70
        };
    }

    public static int GetNObstacles(int nPlayers)
    {
        return nPlayers switch
        {
            <= 2 => 4,
            3 => 2,
            _ => 0
        };
    }

    public static float GetInitialAcceleration(int nPlayers)
    {
        if (nPlayers <= 2)
            return 1.4f;

        return 1.1f;
    }

    public static float GetInitialTurnAcceleration(int nPlayers)
    {
        if (nPlayers <= 2)
            return 1.5f;

        return 1.25f;
    }
}
