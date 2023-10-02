using System.Collections.Generic;
using weave.Controller;

namespace weave;

/// <summary>
/// Data that NEEDS to be transferred between scenes. Only use this for data that is absolutely necessary.
/// </summary>
public static class GameState
{
    /// <summary>
    /// Set of controllers that are currently in use.
    /// </summary>
    public static ISet<IController> Controllers { get; set; } = new HashSet<IController>();

    static GameState()
    {
    }
}
