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

    [GetNode("Label")]
    private Label _label;

    private string _playerId;

    public string PlayerId
    {
        get => _playerId;
        set
        {
            _playerId = value;
            _label.Text = value;
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
        if (body is not Player player)
            return;

        if (player.PlayerId != PlayerId)
            return;

        _label.Text = "Goal!";
        EmitSignal(SignalName.PlayerReachedGoal, player);
    }
}
