namespace weave.Controller;

public interface IController
{
    bool IsTurningLeft();
    bool IsTurningRight();
    int DeviceId => -1;
    public Controller Type { get; }
}
