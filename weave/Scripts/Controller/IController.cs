using System;

namespace weave.Controller;

public interface IController : IEquatable<IController>
{
    int DeviceId => -1;
    public Controller Type { get; }
    bool IsTurningLeft();
    bool IsTurningRight();
}
