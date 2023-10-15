using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.Utils;

namespace Weave;

[Scene("res://Objects/Player.tscn")]
public partial class Player : CharacterBody2D
{
    private bool _isMoving;
    private bool _hasReachedSize;
    private Vector2 _desiredScale = new(1, 1);

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
    public float TurnRadius { get; set; } = 80;

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

        Scale = new Vector2(0, 0);
        SetSize(2, 2);
        AddChild(
            TimerFactory.StartedSelfDestructingOneShot(WeaveConstants.InitialCountdownLength - WeaveConstants.CountdownLength, () => SetSize(1, 1))
        );
    }

    public override void _Process(double delta)
    {
        if (_hasReachedSize)
            return;

        Scale = Scale.Lerp(_desiredScale, (float)delta * 4f);

        // Have not reached size yet
        if (Mathf.Abs(Scale.DistanceTo(_desiredScale)) > 0.01f) return;

        Scale = _desiredScale;
        _hasReachedSize = true;
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
            RotationDegrees += TurnRadius * (float)delta;

        if (PlayerInfo.InputSource.IsTurningLeft())
            RotationDegrees -= TurnRadius * (float)delta;
    }

    public void SetSize(int x, int y)
    {
        _desiredScale = new(x, y);
        _hasReachedSize = false;
    }

    public float GetRadius()
    {
        return CircleShape.Radius;
    }
}