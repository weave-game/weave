using System.Collections.Generic;
using Godot;

namespace Weave.InputSources;

public sealed class WebInputSource : IInputSource
{
    public string Id { get; }

    private readonly Queue<string> _directionQueue = new();
    private string _lastDirection;

    public WebInputSource(string id)
    {
        Id = id;
    }

    private bool IsTurning(string direction)
    {
        if (_directionQueue.Count > 0)
        {
            if (_directionQueue.Peek() != direction)
                return false;

            _lastDirection = _directionQueue.Dequeue();
            return true;
        }
        return _lastDirection == direction;
    }

    public bool IsTurningLeft()
    {
        return IsTurning("left");
    }

    public bool IsTurningRight()
    {
        return IsTurning("right");
    }

    public void SetDirection(string direction)
    {
        if (_directionQueue.Count > 0 && _directionQueue.Peek() == "forward")
            _directionQueue.Dequeue();

        _directionQueue.Enqueue(direction.ToLower());
    }

    public InputType Type => InputType.Web;

    public bool Equals(IInputSource other)
    {
        if (other is WebInputSource web)
        {
            return web.Id == Id;
        }

        return false;
    }

    public string LeftInputString()
    {
        return "web left";
    }

    public string RightInputString()
    {
        return "web right";
    }

    public TextureRect LeftInputIcon()
    {
        var imageResource = GD.Load<CompressedTexture2D>("res://Assets/Icons/phone.svg");
        var icon = new TextureRect() { Texture = imageResource };
        icon.CustomMinimumSize = new Vector2(38, 38);
        icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        return icon;
    }

    public TextureRect RightInputIcon()
    {
        var imageResource = GD.Load<CompressedTexture2D>("res://Assets/Icons/phone.svg");
        var icon = new TextureRect() { Texture = imageResource };
        icon.CustomMinimumSize = new Vector2(38, 38);
        icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        return icon;
    }
}
