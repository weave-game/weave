using System.Collections.Generic;
using System.Linq;
using Godot;
using weave.InputDevices;

namespace weave;

public sealed class Lobby
{
    private readonly IList<IInputDevice> _connectedInputDevices = new List<IInputDevice>();
    public IReadOnlyList<IInputDevice> ConnectedInputDevices => _connectedInputDevices.ToList();
    
    #region Keyboard

    public void ToggleKeyboard((Key, Key) keybindingTuple)
    {
        var kb = new KeyboardInputDevice(keybindingTuple);
        var alreadyExisting = _connectedInputDevices.FirstOrDefault(input => input.Equals(kb));

        if (alreadyExisting == null)
        {
            _connectedInputDevices.Add(kb);
        }
        else
        {
            _connectedInputDevices.Remove(alreadyExisting);
        }
    }

    #endregion

    #region Gamepad

    public void AddGamepad(int deviceId)
    {
        if (deviceId < 0) return;

        if (_connectedInputDevices.Any(c => c.DeviceId == deviceId))
            return;

        _connectedInputDevices.Add(new GamepadInputDevice(deviceId));
    }

    public void RemoveGamepad(int deviceId)
    {
        if (deviceId < 0) return;

        var toRemove = _connectedInputDevices.FirstOrDefault(c => c.DeviceId == deviceId);
        if (toRemove == null)
            return;

        _connectedInputDevices.Remove(toRemove);
    }

    #endregion
}