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

    public bool IsTurning(string direction)
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
        return "< (web)";
    }

    public string RightInputString()
    {
        return "(web) >";
    }
}
