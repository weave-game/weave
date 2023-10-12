namespace Weave;

/// <summary>
///     Wrapper to abstract logic for handling logic for scaling game elements such as player movement speed or acceleration.
/// </summary>
public static class ScalingBrain
{
    public static float GetInitialPlayerMovement(int nPlayers)
    {
        if (nPlayers <= 2)
            return 70;

        return 100;
    }

    public static float GetInitialAcceleration(int nPlayers)
    {
        if (nPlayers <= 2)
            return 1.1f;

        return 1.5f;
    }

    public static float GetInitialTurnAcceleration(int nPlayers)
    {
        if (nPlayers <= 2)
            return 1.25f;

        return 1.5f;
    }
}