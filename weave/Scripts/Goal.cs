using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;

namespace Weave;

[Scene("res://Objects/Goal.tscn")]
public partial class Goal : Node2D
{
    /// <summary>
    ///     This is the signal that will be emitted when a player reaches the goal.
    ///     Will only be emitted once.
    /// </summary>
    [Signal]
    public delegate void PlayerReachedGoalEventHandler();

    private Color _color;
    private bool _locked = true;
    private bool _reached;

    [GetNode("UnlockParticles")]
    private CpuParticles2D _unlockParticles;

    [GetNode("GoalSprite")]
    private Sprite2D _goalSprite;

    [GetNode("LockSprite")]
    private Sprite2D _lockSprite;

    [GetNode("LockAreaSprite")]
    private Sprite2D _lockAreaSprite;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            Modulate = value;
        }
    }

    public override void _Ready()
    {
        this.GetNodes();
        var area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnBodyEntered;

        var lockArea = GetNode<Area2D>("LockArea");
        lockArea.BodyEntered += OnLockAreaBodyEntered;

        _goalSprite.Hide();
    }

    private void OnLockAreaBodyEntered(Node2D body)
    {
        if (!_locked) return;
        if (body is not Player player) return;
        if (player.Color == Color) return;

        _unlockParticles.Emitting = true;
        _locked = false;
        _lockSprite.Visible = false;
        _lockAreaSprite.Visible = false;
        _goalSprite.Show();
        _lockSprite.Hide();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_reached || _locked) return;
        if (body is not Player player) return;
        if (player.Color != Color) return;

        _reached = true;
        _goalSprite.Modulate = Colors.Black;
        EmitSignal(SignalName.PlayerReachedGoal);
        QueueFree();
    }
}