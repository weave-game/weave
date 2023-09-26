using System;
using Godot;

namespace weave;

public partial class FlyingLight : Area2D
{
    private PathFollow2D pathFollow;
    private int Speed = 5;
    private int _step;

    public override void _Ready()
    {
        pathFollow = GetParent<PathFollow2D>();
    }

    public override void _Process(double delta)
    {
        pathFollow.Progress += CalculateProgress();
    }

    private float CalculateProgress()
    {
        return 2f;
    }
}
