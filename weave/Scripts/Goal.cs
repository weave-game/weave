using Godot;

public partial class Goal : Node2D
{
    /// <summary>
    ///   This is the signal that will be emitted when a player reaches the goal.
    ///   Will only be emitted once.
    /// </summary>
    [Signal]
    private delegate void PlayerReachedGoalEventHandler(Player player);

    public override void _Ready()
    {
        var area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player player) return;

        // TODO: Check if flag is associated with this goal...
        EmitSignal(SignalName.PlayerReachedGoal, player);
    }
}