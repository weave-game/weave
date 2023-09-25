using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace weave.Utils;

/// <summary>
///     A utility class designed to provide unique identifiers and colors.
/// </summary>
public static class Unique
{
    // Holds the counter for generating unique IDs.
    private static int _counter = 1;

    /// <summary>
    ///     Some nice looking colors : )
    /// </summary>
    private static readonly ISet<Color> Colors = new HashSet<Color>
    {
        Godot.Colors.Aqua,
        Godot.Colors.Coral
    };

    // Holds the colors that have been used.
    private static readonly ISet<Color> UsedColors = new HashSet<Color>();

    /// <summary>
    ///     Generates a new unique identifier represented as a string.
    /// </summary>
    /// <returns>A string representation of a unique identifier.</returns>
    public static string NewId()
    {
        return _counter++.ToString();
    }

    /// <summary>
    ///     Provides a new unique color that has not been used before.
    ///     If all predefined colors are used, it generates a random color until
    ///     a unique color is found.
    /// </summary>
    /// <returns>A <see cref="Color" /> representing a unique color.</returns>
    public static Color NewColor()
    {
        var unusedColor = Colors.Except(UsedColors).FirstOrDefault();

        if (unusedColor == default)
            while (UsedColors.Contains(unusedColor))
                unusedColor = RandomColor();

        UsedColors.Add(unusedColor);
        return unusedColor;
    }

    /// <summary>
    ///     Generates a random color.
    /// </summary>
    /// <returns>A <see cref="Color" /> object representing a random color.</returns>
    private static Color RandomColor()
    {
        var random = new Random();
        return new Color(
            (float)random.NextDouble(),
            (float)random.NextDouble(),
            (float)random.NextDouble()
        );
    }
}
