using System.Collections.Generic;
using weave.InputDevices;

namespace weave;

/// <summary>
///     Data that NEEDS to be transferred between scenes. Only use this for data that is absolutely necessary.
/// </summary>
public static class GameState
{
    static GameState() { }

    /// <summary>
    ///     Set of input devices that are currently in use.
    /// </summary>
    public static IList<IInputDevice> InputDevices { get; set; } = new List<IInputDevice>();
}
