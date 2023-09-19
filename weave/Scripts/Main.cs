using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.Utils;

namespace weave;

public partial class Main : Node2D
{
    private readonly ISet<Player> _players = new HashSet<Player>();
    private int _nPlayers = 3;

    /// <summary>
    ///     How many players that have reached the goal during the current round.
    /// </summary>
    private int _roundCompletions;

    public override void _Ready()
    {
        this.GetNodes();
        SpawnPlayers(_nPlayers);
        ClearAndSpawnGoals();
    }

    private void SpawnPlayers(int amount)
    {
        amount.TimesDo(() =>
        {
            var playerId = UniqueId.Generate();
            var player = Instanter.Instantiate<Player>();
            _players.Add(player);

            AddChild(player);
            player.GlobalPosition = GetRandomCoordinateInView(100);
            player.PlayerId = playerId;
        });
    }

    private void OnPlayerReachedGoal(Player player)
    {
        GD.Print($"Player {player.PlayerId} has reached the goal");
        _roundCompletions++;

        if (_roundCompletions == _nPlayers)
        {
            GD.Print("All players have reached the goal");

            _roundCompletions = 0;
            ClearAndSpawnGoals();
        }
    }

    private void ClearAndSpawnGoals()
    {
        // Remove existing goals
        GetTree()
            .GetNodesInGroup(GroupConstants.GoalGroup)
            .ToList()
            .ForEach(goal => goal.QueueFree());

        // Spawn new goals
        _players
            .ToList()
            .ForEach(player =>
            {
                var goal = Instanter.Instantiate<Goal>();
                CallDeferred("add_child", goal);
                goal.GlobalPosition = GetRandomCoordinateInView(100);
                goal.PlayerReachedGoal += OnPlayerReachedGoal;
                goal.CallDeferred("set", nameof(Player.PlayerId), player.PlayerId);
            });
    }

    private Vector2 GetRandomCoordinateInView(float margin)
    {
        var x = (float)GD.RandRange(margin, GetViewportRect().Size.X - margin);
        var y = (float)GD.RandRange(margin, GetViewportRect().Size.Y - margin);
        return new Vector2(x, y);
    }
}
