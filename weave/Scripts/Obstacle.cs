using Godot;
using GodotSharper.Instancing;

namespace Weave;

[Scene("res://Objects/Obstacles/Obstacle.tscn")]
public partial class Obstacle : Area2D
{
    private float _desiredXScale = 1;
    private float _desiredYScale = 1;
    private bool _hasReachedSize;
    private bool _isSelfDestructing;

    public override void _Ready()
    {
        AreaEntered += area =>
        {
            if (area.GetParent() is Goal)
                SelfDestruct();
        };
    }

    private void SelfDestruct()
    {
        if (_isSelfDestructing) return;
        _isSelfDestructing = true;

        SetObstacleSize(0, 0);
    }

    public void SetObstacleSize(int x, int y)
    {
        _desiredXScale = x;
        _desiredYScale = y;
        _hasReachedSize = false;
    }

    public override void _Process(double delta)
    {
        if (_hasReachedSize)
            return;

        // Frame independent Lerp according to ChatGPT
        const float baseLerpFactor = 0.1f;
        var t = (float)(1.0f - Mathf.Pow(1.0f - baseLerpFactor, delta * 60.0f));
        var newXScale = Mathf.Lerp(Scale.X, _desiredXScale, t);
        var newYScale = Mathf.Lerp(Scale.Y, _desiredYScale, t);
        Scale = new Vector2(newXScale, newYScale);

        // Have not reached size yet
        if (!(Mathf.Abs(Scale.X - _desiredXScale) < 0.05f) || !(Mathf.Abs(Scale.Y - _desiredYScale) < 0.05f)) return;

        Scale = new Vector2(_desiredXScale, _desiredYScale);
        _hasReachedSize = true;

        if (_isSelfDestructing) QueueFree();
    }
}