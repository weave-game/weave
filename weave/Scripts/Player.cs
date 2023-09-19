using Godot;

public partial class Player : RigidBody2D
{
    [Signal]
    public delegate void PlayerShotBulletEventHandler(Node2D bullet, Vector2 globalPosition);

    private const int MovementSpeed = 100;
    private PackedScene _bombScene = GD.Load<PackedScene>("res://Objects/Bomb.tscn");

    public override void _PhysicsProcess(double delta)
    {
        Move(delta);

        if (Input.IsActionJustPressed("shoot"))
            Shoot();
    }

    private void Move(double delta)
    {
        var input = GetInput();
        Translate(input * MovementSpeed * (float)delta);
    }

    private void Shoot()
    {
        var bomb = _bombScene.Instantiate<RigidBody2D>();
        EmitSignal(SignalName.PlayerShotBullet, bomb, GlobalPosition);
    }

    private static Vector2 GetInput()
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
