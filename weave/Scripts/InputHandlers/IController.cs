namespace weave.InputHandlers;

public interface IController
{
    bool IsTurningLeft();
    bool IsTurningRight();
    int DeviceId => -1;
}
