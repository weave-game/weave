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

    public bool HasLock { get; set; }

    private Color _color;
    private bool _locked;
    private bool _reached;

    [GetNode("UnlockParticles")]
    private CpuParticles2D _unlockParticles;

    [GetNode("CollectPlayer")]
    private AudioStreamPlayer2D _collectSoundPlayer;

    [GetNode("GoalSprite")]
    private Sprite2D _goalSprite;

    [GetNode("LockSprite")]
    private Sprite2D _lockSprite;

    [GetNode("UnlockPlayer")]
    private AudioStreamPlayer2D _unlockSoundPlayer;

    [GetNode("UnlockAreaSprite")]
    private Sprite2D _unlockAreaSprite;

    public IList<Color> OtherColors;
    private float _unlockDrawingRotation;
    private float _unlockAreaRadius;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;

            _lockSprite.Modulate = value;
            _goalSprite.Modulate = value;
            _unlockAreaSprite.Modulate = value;
        }
    }

    public override void _Ready()
    {
        this.GetNodes();
        var area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnBodyEntered;

        HasLock = true;
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
        if (!HasLock) return;

        _unlockDrawingRotation += 0.1f * (float)delta;
        QueueRedraw();
    }

    private void OnLockAreaBodyEntered(Node2D body)
    {
        if (!_locked) return;
        if (body is not Player player) return;
        if (player.PlayerInfo.Color == Color) return;

        _unlockParticles.Emitting = true;
        _locked = false;
        _lockSprite.Visible = false;
        _unlockAreaSprite.Visible = false;
        _goalSprite.Show();
        _lockSprite.Hide();
        _unlockSoundPlayer.Play();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_reached || _locked) return;
        if (body is not Player player) return;
        if (player.PlayerInfo.Color != Color) return;

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
            throw new NodeNotFoundException("Unlock area collision shape is not a circle shape.");

        return circleShape.Radius;
    }

    public override void _Draw()
    {
        if (!_locked) return;

        if (OtherColors == null || OtherColors.Count == 0)
            return;

        var center = new Vector2(0, 0);
        var totalColors = OtherColors.Count;
        var arcAngle = 2 * (float)Math.PI / totalColors;

        for (var i = 0; i < totalColors; i++)
        {
            var startAngle = (i * arcAngle) + _unlockDrawingRotation;
            var endAngle = ((i + 1) * arcAngle) + _unlockDrawingRotation;
            DrawArc(center, _unlockAreaRadius, startAngle, endAngle, 32, OtherColors[i], 8);
        }
    }
}