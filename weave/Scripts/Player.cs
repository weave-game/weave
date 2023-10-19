using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.Utils;

namespace Weave;

[Scene("res://Objects/Player.tscn")]
public partial class Player : CharacterBody2D
{
    [GetNode("Arrow")]
    private Sprite2D _arrow;

    private Vector2 _desiredScale = new(1, 1);
    private bool _hasReachedSize;
    private bool _isMoving;

    [GetNode("PlayerNamePivot/PlayerName")]
    private Label _playerName;

    [GetNode("PlayerNamePivot")]
    private Node2D _playerNamePivot;

    [GetNode("Sprite2D")]
    private Sprite2D _sprite2D;

    [GetNode("HorizontalIndicator")]
    private Sprite2D _indicatorX;

    [GetNode("VerticalIndicator")]
    private Sprite2D _indicatorY;

    public PlayerInfo PlayerInfo { get; set; }

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
        _indicatorX.Modulate = PlayerInfo.Color;
        _indicatorY.Modulate = PlayerInfo.Color;
        _playerName.Text = PlayerInfo.Name;
        _arrow.Modulate = PlayerInfo.Color;

        Scale = new(0, 0);
        SetSize(2, 2);
        AddChild(
            TimerFactory.StartedSelfDestructingOneShot(WeaveConstants.InitialCountdownLength - WeaveConstants.CountdownLength, () => SetSize(1, 1))
        );
    }

    public override void _Process(double delta)
    {
        if (!_hasReachedSize)
        {
            Scale = Scale.Lerp(_desiredScale, (float)delta * 4f);

            if (Mathf.Abs(Scale.DistanceTo(_desiredScale)) < 0.01f)
            {
                _hasReachedSize = true;
            }
        }

        HandleIndicators();
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
        {
            Translate(Vector2.Up.Rotated(Rotation).Normalized() * MovementSpeed * (float)delta);
        }
    }

    private void Rotate(double delta)
    {
        if (!IsTurning)
        {
            return;
        }

        if (PlayerInfo.InputSource.IsTurningRight())
        {
            RotationDegrees += TurnSpeed * (float)delta;
        }

        if (PlayerInfo.InputSource.IsTurningLeft())
        {
            RotationDegrees -= TurnSpeed * (float)delta;
        }
    }

    private void HandleIndicators()
    {
        const float MinDistance = 200;
        const float MaxScale = 0.003f;

        var width = GetViewportRect().Size.X;
        var height = GetViewportRect().Size.Y;

        var distance = new Vector2(
            Mathf.Min(GlobalPosition.X, width - GlobalPosition.X),
            Mathf.Min(GlobalPosition.Y, height - GlobalPosition.Y)
        );

        _indicatorX.GlobalPosition = new(
            (Mathf.Round(GlobalPosition.X / width) + 1) % 2 * width,
            GlobalPosition.Y
        );

        _indicatorY.GlobalPosition = new(
            GlobalPosition.X,
            (Mathf.Round(GlobalPosition.Y / height) + 1) % 2 * height
        );

        float scaleX = Mathf.Pow(Mathf.Max(0, (MinDistance - distance.X) / MinDistance), 2) * MaxScale;
        float scaleY = Mathf.Pow(Mathf.Max(0, (MinDistance - distance.Y) / MinDistance), 2) * MaxScale;

        _indicatorX.GlobalScale = new(scaleX, scaleX);
        _indicatorY.GlobalScale = new(scaleY, scaleY);
    }

    private void SetSize(int x, int y)
    {
        _desiredScale = new(x, y);
        _hasReachedSize = false;
    }

    public float GetRadius()
    {
        return CircleShape.Radius;
    }
}
