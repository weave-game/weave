using System;
using Godot;

namespace Weave.InputSources;

public sealed class KeyboardInputSource : IInputSource
{
    private readonly Key _left;
    private readonly Key _right;

    private readonly PackedScene _keyIcon = GD.Load<PackedScene>("res://Objects/KeyIcon.tscn");

    public KeyboardInputSource((Key, Key) keybindings)
    {
        _left = keybindings.Item1;
        _right = keybindings.Item2;
    }

    public bool IsTurningLeft()
    {
        return Input.IsKeyPressed(_left);
    }

    public bool IsTurningRight()
    {
        return Input.IsKeyPressed(_right);
    }

    public InputType Type => InputType.Keyboard;

    public string LeftInputString()
    {
        if (_left.ToString().Contains("Key"))
        {
            return _left.ToString()[3..];
        }

        if (string.Equals(_left.ToString(), "left", StringComparison.InvariantCultureIgnoreCase))
        {
            return "←";
        }

        return _left.ToString();
    }

    public string RightInputString()
    {
        if (_right.ToString().Contains("Key"))
        {
            return _right.ToString()[3..];
        }

        if (string.Equals(_right.ToString(),"right", StringComparison.InvariantCultureIgnoreCase))
        {
            return "→";
        }

        return _right.ToString();
    }

    public TextureRect LeftInputIcon()
    {
            var leftKeyIcon = _keyIcon.Instantiate<TextureRect>();
            leftKeyIcon.GetNode<Label>("Label").Text = LeftInputString();
            return leftKeyIcon;
    }

    public TextureRect RightInputIcon()
    {
        var rightKeyIcon = _keyIcon.Instantiate<TextureRect>();
        rightKeyIcon.GetNode<Label>("Label").Text = RightInputString();
        return rightKeyIcon;
    }

    public bool Equals(IInputSource other)
    {
        if (other is KeyboardInputSource keyboard)
        {
            return keyboard._left == _left && keyboard._right == _right;
        }

        return false;
    }
}
