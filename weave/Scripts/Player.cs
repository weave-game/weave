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
    private const int MovementSpeed = 100;

    [GetNode("Label")]
    private Label _label;

    [GetNode("CurveSpawner")]
    public CurveSpawner CurveSpawner { get; set; }
    private string _playerId;

    [GetNode("CollisionShape2D")]
    private CollisionShape2D _collisionShape2D;

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
    }

    public override void _PhysicsProcess(double delta)
    {
        Move(delta);
        CurveSpawner.Step(GlobalPosition, Rotation, GetRadius());
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
        var shape = (CircleShape2D)_collisionShape2D.Shape;
        return shape.Radius;
    }
}
