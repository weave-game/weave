using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.Utils;

namespace weave;

[Instantiable(ObjectResources.GoalScene)]
public partial class Goal : Node2D
{
    /// <summary>
    ///     This is the signal that will be emitted when a player reaches the goal.
    ///     Will only be emitted once.
    /// </summary>
    [Signal]
    public delegate void PlayerReachedGoalEventHandler(Player player);

    [GetNode("Sprite2D")]
    private Sprite2D _sprite;

    private bool _reached;
    private Color _color;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            Modulate = value;
            GD.Print($"Goal color set to {value}");
            GD.Print("And turqouise is a nice color", Colors.Turquoise);
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
        EmitSignal(SignalName.PlayerReachedGoal, player);
    }
}