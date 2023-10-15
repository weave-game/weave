using Godot;
using Weave.InputSources;

namespace Weave.Utils;

public class PlayerInfo
{
    public string Name { get; set; }
    public Color Color { get; set; }
    public IInputSource InputSource { get; init; }
}