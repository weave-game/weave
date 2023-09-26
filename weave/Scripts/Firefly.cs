using Godot;
using System;
using GodotSharper.AutoGetNode;
using System.Reflection.Emit;

namespace weave;

public partial class Firefly : Path2D
{
    [GetNode("PathFollow2D")]
    private PathFollow2D pathFollow;

    [GetNode("PathFollow2D/Area2D")]
    private Area2D area;

    [GetNode("Line2D")]
    private Line2D line;

    private float Speed;
    private float GoalSpeed;
    private const int NrPoints = 20;
    private float distanceBetweenPoints = 5;

    public override void _Ready()
    {
        this.GetNodes();

        line.Width = 1;

        for (var i = 0; i < NrPoints; i++)
        {
            line.AddPoint(new Vector2(Position.X, Position.Y));
        }
    }

    public override void _Process(double delta)
    {
        if (MathF.Abs(Speed - GoalSpeed) < (float)10e-5)
        {
            GoalSpeed = GD.Randf() * 10;
        }

        if (line.Points[0].DistanceTo(area.GlobalPosition) >= distanceBetweenPoints)
        {
            var lastPoint = area.GlobalPosition;
            var tempPoints = (Vector2[])line.Points.Clone();

            for (var i = 0; i < NrPoints; i++)
            {
                tempPoints[i].X = lastPoint.X;
                tempPoints[i].Y = lastPoint.Y;
                if (i > 0)
                    lastPoint = line.Points[i - 1];
            }

            line.Points = tempPoints;
        }

        Speed = Mathf.Lerp(Speed, GoalSpeed, 0.3f);
        pathFollow.Progress += Speed;
    }
}
