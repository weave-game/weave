using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.Scripts.Utils;

public partial class Main : Node2D
{
    private int _players = 3;

    public override void _Ready()
    {
        this.GetNodes();
        SpawnPlayers(_players);
    }

    private void SpawnPlayers(int amount)
    {
        amount.TimesDo(() =>
        {
            var playerId = UniqueId.Generate();
            var player = Instanter.Instantiate<Player>();
            var goal = Instanter.Instantiate<Goal>();

            AddChild(player);
            player.GlobalPosition = GetRandomCoordinateInView();
            player.PlayerId = playerId;

            AddChild(goal);
            goal.GlobalPosition = GetRandomCoordinateInView();
            goal.PlayerReachedGoal += OnPlayerReachedGoal;
            goal.PlayerId = playerId;
        });
    }

    private static void OnPlayerReachedGoal(Player player)
    {
        GD.Print($"Player {player.PlayerId} has reached the goal");
    }

    private Vector2 GetRandomCoordinateInView()
    {
        var x = (float)GD.RandRange(0, GetViewportRect().Size.X);
        var y = (float)GD.RandRange(0, GetViewportRect().Size.Y);
        return new Vector2(x, y);
    }
}