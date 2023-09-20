using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.Utils;

namespace weave;

[Instantiable(ObjectResources.PlayerScene)]
public partial class Player : RigidBody2D
{
    [Signal]
    public delegate void PlayerShotBulletEventHandler(Node2D bullet, Vector2 globalPosition);

    private const int MovementSpeed = 500;

    [GetNode("Label")]
    private Label _label;

    private string _playerId;

    public string PlayerId
    {
        get => _playerId;
        set
        {
            _label.Text = _playerId;
            _playerId = value;
        }
    }

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _PhysicsProcess(double delta)
    {
        Move(delta);
    }

    private void Move(double delta)
    {
        var input = GetInput();
        Translate(input * MovementSpeed * (float)delta);
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
