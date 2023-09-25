using System.Collections.Generic;
using System.Linq;
using Godot;

namespace weave.Utils;

/// <summary>
///     A utility class designed to provide unique identifiers and colors.
/// </summary>
public static class Unique
{
    /// <summary>
    ///     Some nice looking colors : )
    /// </summary>
    private static readonly ISet<Color> Colors = new HashSet<Color>
    {
        Godot.Colors.Turquoise,
        Godot.Colors.Magenta,
        Godot.Colors.SpringGreen,
        Godot.Colors.OrangeRed,
        Godot.Colors.Gold,
        Godot.Colors.BlueViolet,
        Godot.Colors.HotPink,
        Godot.Colors.DarkOrange
    };

    private static readonly ISet<Color> UsedColors = new HashSet<Color>();

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
            do
            {
                unusedColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
            } while (UsedColors.Contains(unusedColor));

        UsedColors.Add(unusedColor);
        return unusedColor;
    }
}
