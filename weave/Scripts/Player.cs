using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.Utils;

namespace Weave;

[Scene("res://Objects/Player.tscn")]
public partial class Player : CharacterBody2D
{
    private bool _isMoving;

    public PlayerInfo PlayerInfo { get; set; }

    [GetNode("Sprite2D")]
    private Sprite2D _sprite2D;

    [GetNode("PlayerNamePivot")]
    private Node2D _playerNamePivot;

    [GetNode("PlayerNamePivot/PlayerName")]
    private Label _playerName;

    [GetNode("Arrow")]
    private Sprite2D _arrow;

    public float MovementSpeed { get; set; }

    /// <summary>
    ///     Turn speed in degrees.
    /// </summary>
    public float TurnSpeed { get; set; } = 120;

    private CircleShape2D CircleShape { get; set; }

    [GetNode("CurveSpawner")]
    public CurveSpawner CurveSpawner { get; private set; }

    [GetNode("CollisionShape2D")]
    public CollisionShape2D CollisionShape2D { get; private set; }

    public bool IsMoving
    {
        get => _isMoving;
        set
        {
            _isMoving = value;
            _arrow.Visible = !value;
            CurveSpawner.ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        }
    }

    public bool IsTurning { get; set; }

    public override void _Ready()
    {
        this.GetNodes();
        CircleShape = CollisionShape2D.Shape as CircleShape2D;
        CurveSpawner.Color = PlayerInfo.Color;
        _sprite2D.Modulate = PlayerInfo.Color;
        _playerName.Text = PlayerInfo.Name;
        _arrow.Modulate = PlayerInfo.Color;
    }

    public override void _PhysicsProcess(double delta)
    {
        Rotate(delta);
        Move(delta);
        _playerNamePivot.RotationDegrees = -RotationDegrees;
    }

    private void Move(double delta)
    {
        if (_isMoving)
            Translate(Vector2.Up.Rotated(Rotation).Normalized() * MovementSpeed * (float)delta);
    }

    private void Rotate(double delta)
    {
        if (!IsTurning) return;

        if (PlayerInfo.InputSource.IsTurningRight())
            RotationDegrees += TurnSpeed * (float)delta;

        if (PlayerInfo.InputSource.IsTurningLeft())
            RotationDegrees -= TurnSpeed * (float)delta;
    }

    public float GetRadius()
    {
        return CircleShape.Radius;
    }
}