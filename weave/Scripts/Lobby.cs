using System.Collections.Generic;
using System.Linq;
using Godot;
using weave.InputDevices;

namespace weave;

/// <summary>
///     Represents a lobby where players can connect input devices and play games.
/// </summary>
public sealed class Lobby
{
    private readonly IList<IInputDevice> _connectedInputDevices = new List<IInputDevice>();

    /// <summary>
    ///     Gets a read-only list of the input devices currently connected to the lobby.
    /// </summary>
    public IReadOnlyList<IInputDevice> ConnectedInputDevices => _connectedInputDevices.ToList();

    #region Keyboard

    /// <summary>
    ///     Toggles the connection of a keyboard input device with the specified keybinding tuple.
    /// </summary>
    /// <param name="keybindingTuple">The tuple of keys that the keyboard input device should use for input.</param>
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

    /// <summary>
    ///     Adds a gamepad input device with the specified device ID to the lobby.
    /// </summary>
    /// <param name="deviceId">The ID of the gamepad input device to add.</param>
    public void AddGamepad(int deviceId)
    {
        if (deviceId < 0)
            return;

        if (_connectedInputDevices.Any(c => c.DeviceId == deviceId))
            return;

        _connectedInputDevices.Add(new GamepadInputDevice(deviceId));
    }

    /// <summary>
    ///     Removes the gamepad input device with the specified device ID from the lobby.
    /// </summary>
    /// <param name="deviceId">The ID of the gamepad input device to remove.</param>
    public void RemoveGamepad(int deviceId)
    {
        if (deviceId < 0)
            return;

        var toRemove = _connectedInputDevices.FirstOrDefault(c => c.DeviceId == deviceId);
        if (toRemove == null)
            return;

        _connectedInputDevices.Remove(toRemove);
    }

    #endregion
}
