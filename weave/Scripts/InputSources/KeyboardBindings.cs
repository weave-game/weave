using System.Collections.Generic;
using Godot;

namespace Weave.InputSources;

public static class KeyboardBindings
{
    public static readonly IReadOnlyList<(Key, Key)> Keybindings = new List<(Key, Key)>
    {
        (Key.Left, Key.Right), (Key.Key1, Key.Q), (Key.B, Key.N), (Key.Z, Key.X)
    };
}
