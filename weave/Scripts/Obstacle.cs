using Godot;
using GodotSharper.Instancing;

namespace Weave;

[Scene("res://Objects/Obstacles/Obstacle.tscn")]
public partial class Obstacle : Area2D
{
    private Vector2 _desiredScale = new(1, 1);
    private bool _hasReachedSize;
    private bool _isSelfDestructing;

    public override void _Ready()
    {
        AreaEntered += area =>
        {
            if (area.GetParent() is Goal)
            {
                SelfDestruct();
            }
        };
    }

    private void SelfDestruct()
    {
        if (_isSelfDestructing)
        {
            return;
        }

        _isSelfDestructing = true;

        SetObstacleSize(0, 0);
    }

    public void SetObstacleSize(int x, int y)
    {
        _desiredScale = new(x, y);
        _hasReachedSize = false;
    }

    public override void _Process(double delta)
    {
        if (_hasReachedSize)
        {
            return;
        }

        Scale = Scale.Lerp(_desiredScale, (float)delta * 6f);

        // Have not reached size yet
        if (Mathf.Abs(Scale.DistanceTo(_desiredScale)) > 0.01f)
        {
            return;
        }

        Scale = _desiredScale;
        _hasReachedSize = true;

        if (_isSelfDestructing)
        {
            QueueFree();
        }
    }
}
