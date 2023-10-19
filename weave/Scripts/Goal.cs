using System;
using System.Collections.Generic;
using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Exceptions;
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

    [GetNode("CollectPlayer")]
    private AudioStreamPlayer2D _collectSoundPlayer;

    [GetNode("PlayerNameLabel")]
    private Label _playerNameLabel;

    private Color _color;

    [GetNode("GoalSprite")]
    private Sprite2D _goalSprite;

    private bool _locked;

    [GetNode("LockSprite")]
    private Sprite2D _lockSprite;

    private bool _reached;
    private float _unlockAreaRadius;

    [GetNode("UnlockAreaSprite")]
    private Sprite2D _unlockAreaSprite;

    private float _unlockDrawingRotation;
    private string _playerName;

    [GetNode("UnlockParticles")]
    private CpuParticles2D _unlockParticles;

    [GetNode("UnlockPlayer")]
    private AudioStreamPlayer2D _unlockSoundPlayer;

    public bool HasLock { get; set; }

    /// <summary>
    ///     The colors that the unlock area circle should display.
    /// </summary>
    public IList<Color> UnlockAreaColors { get; set; }

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;

            _goalSprite.Modulate = value;
            _lockSprite.Modulate = value;
            _unlockAreaSprite.Modulate = value;
        }
    }

    public override void _Ready()
    {
        this.GetNodes();
        var area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnBodyEntered;
        _playerNameLabel.Text = _playerName;

        if (HasLock)
        {
            _locked = true;
            _unlockAreaRadius = GetUnlockAreaRadius();

            var unlockArea = GetNode<Area2D>("UnlockArea");
            unlockArea.BodyEntered += OnLockAreaBodyEntered;
            _goalSprite.Hide();
        }
        else
        {
            _locked = false;
            _lockSprite.Hide();
            _unlockAreaSprite.Hide();
        }
    }

    public override void _Process(double delta)
    {
        if (!HasLock)
        {
            return;
        }

        _unlockDrawingRotation += 0.5f * (float)delta;
        QueueRedraw();
    }

    private void OnLockAreaBodyEntered(Node2D body)
    {
        if (!_locked)
        {
            return;
        }

        if (body is not Player player)
        {
            return;
        }

        if (player.PlayerInfo.Color == Color)
        {
            return;
        }

        _unlockParticles.Emitting = true;
        _locked = false;
        _lockSprite.Visible = false;
        _unlockAreaSprite.Visible = false;
        _goalSprite.Show();
        _lockSprite.Hide();
        _unlockSoundPlayer.Play();
    }

    public void SetPlayerName(string playerName)
    {
        if (_playerNameLabel != null)
            _playerNameLabel.Text = playerName;

        _playerName = playerName;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_reached || _locked)
        {
            return;
        }

        if (body is not Player player)
        {
            return;
        }

        if (player.PlayerInfo.Color != Color)
        {
            return;
        }

        _reached = true;
        _goalSprite.Modulate = Colors.Black;
        EmitSignal(SignalName.PlayerReachedGoal);

        _collectSoundPlayer.Finished += QueueFree;
        _collectSoundPlayer.Play();
    }

    private float GetUnlockAreaRadius()
    {
        var unlockAreaCollisionShape = GetNode<CollisionShape2D>("UnlockArea/CollisionShape2D");

        if (unlockAreaCollisionShape.Shape is not CircleShape2D circleShape)
        {
            throw new NodeNotFoundException("Unlock area collision shape is not a circle shape.");
        }

        return circleShape.Radius;
    }

    public override void _Draw()
    {
        if (!_locked)
        {
            return;
        }

        if (UnlockAreaColors == null || UnlockAreaColors.Count == 0)
        {
            return;
        }

        var center = new Vector2(0, 0);
        var totalColors = UnlockAreaColors.Count;
        var arcAngle = (2 * (float)Math.PI) / totalColors;

        for (var i = 0; i < totalColors; i++)
        {
            var startAngle = (i * arcAngle) + _unlockDrawingRotation;
            var endAngle = ((i + 1) * arcAngle) + _unlockDrawingRotation;
            DrawArc(center, _unlockAreaRadius - 8, startAngle, endAngle, 32, UnlockAreaColors[i], 8);
        }
    }
}
