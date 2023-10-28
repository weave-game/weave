using System;

namespace Weave.Utils;

/// <summary>
///     GodotSharper random utils.
///     TODO: Move to GodotSharper.
/// </summary>
public static class GsRandom
{
    private static readonly Random s_random = new();

    /// <summary>
    ///     50/50 chance of returning true or false.
    /// </summary>
    /// <returns>
    ///     True or false.
    /// </returns>
    public static bool CoinToss()
    {
        return s_random.Next(2) == 0;
    }
}
