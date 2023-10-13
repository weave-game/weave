using Weave.InputSources;

namespace Weave;

/// <summary>
///     Game configuration.
///     Only use static fields to pass data that NEEDS to be transferred between scenes.
/// </summary>
public static class GameConfig
{
    public static Lobby Lobby { get; set; }

    public static float GetInitialPlayerMovement(int nPlayers)
    {
        if (nPlayers <= 2)
            return 100;

        return 70;
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
