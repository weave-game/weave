using System;
using Godot;
using GodotSharper.AutoGetNode;

public partial class Main : Node2D
{
    private Vector2 start;
    private Vector2 end;
    private bool drawing = false;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                start = mouseButton.Position;
                drawing = true;
            }
            else
            {
                drawing = false;
            }
        }
        else if (@event is InputEventMouseMotion mouseEvent && drawing)
        {
            end = mouseEvent.Position;
            DrawCollisionLine(start, end);
            start = end;
        }
    }

    private void DrawCollisionLine(Vector2 from, Vector2 to)
    {
        // Draw line
        var line = new Line2D();
        line.AddPoint(from);
        line.AddPoint(to);
        AddChild(line);

        // Add collision to line
        var collisionShape = new CollisionShape2D
        {
            DebugColor = new(1,0,0,0),
            Shape = new SegmentShape2D
            {
                A = from,
                B = to
            }
        };

        line.AddChild(collisionShape);
    }

    private void HandleCollision(Node2D body)
    {
        GD.Print("Body entered collision shape");
    }
}
