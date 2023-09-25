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
        _step++;
        // Calculate the sine value of the input x, then multiply by a random factor.
        float sineValue = Mathf.Sin(_step) + 1;
        float randomFactor = GD.Randf(); // Random value between 0 and 1

        // Ensure the result is always positive.
        return Mathf.Abs(sineValue * randomFactor * 10);
    }
}
