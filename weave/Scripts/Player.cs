using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.InputHandlers;
using weave.Utils;

namespace weave;

[Instantiable(ObjectResources.PlayerScene)]
public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void PlayerShotBulletEventHandler(Node2D bullet, Vector2 globalPosition);

    private float MovementSpeed = 100f;

    [GetNode("Label")]
    private Label _label;

    private string _playerId;

    public CircleShape2D CircleShape { get; private set; }

    [GetNode("CurveSpawner")]
    public CurveSpawner CurveSpawner { get; private set; }

    [GetNode("CollisionShape2D")]
    public CollisionShape2D CollisionShape2D;

    public IController Controller { get; set; }

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
        CircleShape = CollisionShape2D.Shape as CircleShape2D;
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

    public float GetRadius()
    {
        var shape = (CircleShape2D)CollisionShape2D.Shape;
        return shape.Radius;
    }
}
