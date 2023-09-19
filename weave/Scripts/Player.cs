using Godot;

public partial class Player : RigidBody2D
{
    [Signal]
    public delegate void PlayerShotBulletEventHandler(Node2D bullet, Vector2 globalPosition);

    private const int MovementSpeed = 100;
    private PackedScene _bombScene = GD.Load<PackedScene>("res://Objects/Bomb.tscn");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        ApplyTorqueImpulse(1000 * (float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        var input = GetInput();
        Translate(input * MovementSpeed * (float)delta);

        if (Input.IsActionJustPressed("shoot"))
            Shoot();
    }

    private void Shoot()
    {
        RigidBody2D bomb = _bombScene.Instantiate<RigidBody2D>();
        // AddChild(bomb); // <- no
        EmitSignal(SignalName.PlayerShotBullet, bomb, GlobalPosition);
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
