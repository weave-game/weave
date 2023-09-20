using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.Utils;
using weave.InputHandlers;

namespace weave;

[Instantiable(ObjectResources.PlayerScene)]
public partial class Player : RigidBody2D
{
    [Signal]
    public delegate void PlayerShotBulletEventHandler(Node2D bullet, Vector2 globalPosition);

    private const int MovementSpeed = 200;

    [GetNode("Label")]
    private Label _label;


    public IController Controller { get; set; }

    private string _playerId;

    public string PlayerId
    {
        get => _playerId;
        set
        {
            _label.Text = value;
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
        Rotate(delta);
        Translate(Vector2.Up.Rotated(Rotation).Normalized() * MovementSpeed * (float)delta);
    }

    private void Rotate(double delta)
    {
        if (Controller.IsTurningRight())
            RotationDegrees += 120 * (float)delta;
        
        if (Controller.IsTurningLeft())
            RotationDegrees -= 120 * (float)delta;
    }
}
