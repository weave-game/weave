using Godot;

public partial class Player : RigidBody2D
{
    private const int MovementSpeed = 100;

    public override void _PhysicsProcess(double delta)
    {
        var input = GetInput();
        Translate(input * MovementSpeed * (float)delta);
    }

    private Vector2 GetInput()
    {
        var input = new Vector2();

        if (Input.IsActionPressed("move_right"))
            input.X += 1;
        if (Input.IsActionPressed("move_left"))
            input.X -= 1;
        if (Input.IsActionPressed("move_down"))
            input.Y += 1;
        if (Input.IsActionPressed("move_up"))
            input.Y -= 1;

        return input.Normalized();
    }
}