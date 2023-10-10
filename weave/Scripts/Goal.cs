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

    private bool _reached;

    [GetNode("Sprite2D")]
    private Sprite2D _sprite;

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
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_reached)
            return;

        if (body is not Player player)
            return;

        if (player.Color != Color)
            return;

        _reached = true;
        _sprite.Modulate = Colors.Black;
        EmitSignal(SignalName.PlayerReachedGoal);
        QueueFree();
    }
}
