using System;

namespace weave.InputDevices;

public interface IInputDevice : IEquatable<IInputDevice>
{
    int DeviceId => -1;
    public InputType Type { get; }
    bool IsTurningLeft();
    bool IsTurningRight();
}
