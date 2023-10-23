using System;
using Godot;

namespace Weave.InputSources;

public interface IInputSource : IEquatable<IInputSource>
{
    public InputType Type { get; }
    bool IsTurningLeft();
    bool IsTurningRight();
    string LeftInputString();
    string RightInputString();
    TextureRect LeftInputIcon();
    TextureRect RightInputIcon();
}
