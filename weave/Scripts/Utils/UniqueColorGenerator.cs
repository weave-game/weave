using System.Collections.Generic;
using System.Linq;
using Godot;

namespace weave.Utils;

/// <summary>
///     Generates unique colors.
/// </summary>
public sealed class UniqueColorGenerator
{
    /// <summary>
    ///     Some nice looking colors : )
    /// </summary>
    private readonly IReadOnlyList<Color> _defaultColors = new List<Color>
    {
        Colors.Red,
        Colors.Seashell,
        Colors.Green,
        Colors.Blue
    };

    private readonly ISet<Color> _usedColors = new HashSet<Color>();

    /// <summary>
    ///     Generates a new unique color that has not been used before.
    /// </summary>
    /// <returns>A new unique color.</returns>
    public Color NewColor()
    {
        var unusedColor = _defaultColors.Except(_usedColors).FirstOrDefault();

        if (unusedColor == default)
        {
            do
            {
                unusedColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
            } while (_usedColors.Contains(unusedColor));
        }

        _usedColors.Add(unusedColor);
        return unusedColor;
    }
}
