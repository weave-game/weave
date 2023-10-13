using System;

namespace Weave.InputSources;

public interface IInputSource : IEquatable<IInputSource>
{
    int DeviceId => -1;
    public InputType Type { get; }
    bool IsTurningLeft();
    bool IsTurningRight();
    string LeftInputString();
    string RightInputString();
}
