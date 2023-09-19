using Godot;
using GodotSharper.AutoGetNode;

public partial class Main : Node2D
{
    [GetNode("MegaCoolLabel")]
    private Label _label;

    [GetNode("KillArea")]
    private Area2D _area2D;

    [GetNode("Player")]
    private Player _player;

    [GetNode("Goal")]
    private Goal _goal;

    public override void _Ready()
    {
        this.GetNodes();
        _label.Text = "Hello World!";

        _area2D.BodyEntered += OnAreaEntered;
        _goal.PlayerReachedGoal += OnPlayerReachedGoal;
    }
    

    private void OnPlayerReachedGoal(Player player)
    {
        GD.Print("Player has reached the goal");
    }

    private void OnAreaEntered(Node2D node)
    {
        if (node is not Player player) return;

        GD.Print("player entered the area");
        player.QueueFree();
    }
}
