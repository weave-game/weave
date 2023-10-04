using System;

namespace weave.InputSources;

public interface IInputSource : IEquatable<IInputSource>
{
    int DeviceId => -1;
    public InputType Type { get; }
    bool IsTurningLeft();
    bool IsTurningRight();
}
