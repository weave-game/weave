using System;
using Godot;

namespace weave;

public partial class FlyingLight : Area2D
{
    private PathFollow2D pathFollow;
    private float Speed;
    private float GoalSpeed;
    private int _step;

    private float lastSpeed;
    private float goalSpeed;

    public override void _Ready()
    {
        pathFollow = GetParent<PathFollow2D>();
    }

    public override void _Process(double delta)
    {
        if (MathF.Abs(Speed - GoalSpeed) < (float) 10e-5)
        {
            GoalSpeed = GD.Randf() * 10;
        }
        Speed = Mathf.Lerp(Speed, GoalSpeed, 0.3f);
        pathFollow.Progress += Speed;
        _step++;
    }
}
