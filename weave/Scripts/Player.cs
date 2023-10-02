using System.Diagnostics;
using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.Controller;
using weave.Utils;

namespace weave;

[Instantiable(ObjectResources.PlayerScene)]
public partial class Player : CharacterBody2D
{
    public float MovementSpeed { get; set; } = 100;
    public float TurnRadius { get; set; } = 120;

    private CircleShape2D CircleShape { get; set; }

    [GetNode("CurveSpawner")]
    public CurveSpawner CurveSpawner { get; private set; }

    [GetNode("CollisionShape2D")]
    public CollisionShape2D CollisionShape2D { get; private set; }

    [GetNode("Sprite2D")]
    private Sprite2D _sprite2D;

    public IController Controller { get; set; }

    public Color Color { get; set; }
    private bool _isMoving;
    public bool IsMoving
    {
        get { return _isMoving; }
        set
        {
            _isMoving = value;
            CurveSpawner.ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        }
    }

    public override void _Ready()
    {
        this.GetNodes();
        CircleShape = CollisionShape2D.Shape as CircleShape2D;
        CurveSpawner.Color = Color;
        _sprite2D.Modulate = Color;
    }

    public override void _PhysicsProcess(double delta)
    {
        Move(delta);
    }

    private void Move(double delta)
    {
        Rotate(delta);
        if (_isMoving)
            Translate(Vector2.Up.Rotated(Rotation).Normalized() * MovementSpeed * (float)delta);
    }

    private void Rotate(double delta)
    {
        if (Controller.IsTurningRight())
            RotationDegrees += TurnRadius * (float)delta;

        if (Controller.IsTurningLeft())
            RotationDegrees -= TurnRadius * (float)delta;
    }

    public float GetRadius()
    {
        return CircleShape.Radius;
    }
}
