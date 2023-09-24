using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.InputHandlers;
using weave.Utils;

namespace weave;

internal enum ControllerTypes
{
    Keyboard,
    Controller // TODO: implement
}

public partial class Main : Node2D
{
    private readonly ISet<Player> _players = new HashSet<Player>();
    private readonly int _nPlayers = 1;
    private readonly int _nrGridColumns = 10;
    private readonly int _nrGridRows = 10;
    private readonly int _width = 1280;
    private readonly int _height = 720;
    private Grid _grid;
    private ControllerTypes _controllerType = ControllerTypes.Keyboard;

    private readonly List<(Key, Key)> _keybindings =
        new() { (Key.Left, Key.Right), (Key.Key1, Key.Q), (Key.B, Key.N), (Key.Z, Key.X) };

    /// <summary>
    ///     How many players that have reached the goal during the current round.
    /// </summary>
    private int _roundCompletions;

    public override void _Ready()
    {
        this.GetNodes();
        if (_keybindings.Count < _nPlayers)
        {
            throw new ArgumentException(
                "More players than available keybindings",
                nameof(_nPlayers)
            );
        }

        CreateMapGrid();
        SpawnPlayers();
        ClearAndSpawnGoals();
    }

    public override void _PhysicsProcess(double delta)
    {
        DetectPlayerCollision();
    }

    private void CreateMapGrid()
    {
        _grid = new Grid(_nrGridRows, _nrGridColumns, _width, _height);
    }

    private void DetectPlayerCollision()
    {
        // Perform collision detection for players that are drawing
        foreach (var player in _players.Where(player => player.CurveSpawner.IsDrawing))
        {
            var segments = _grid.GetSegmentsFromPlayerPosition(
                player.GlobalPosition,
                player.GetRadius()
            );
            if (IsPlayerIntersecting(player, segments))
            {
                GD.Print("Player has collided");
            }
        }
    }

    private void SpawnPlayers()
    {
        var i = 0;
        _nPlayers.TimesDo(() =>
        {
            var playerId = UniqueId.Generate();
            var player = Instanter.Instantiate<Player>();
            if (_controllerType == ControllerTypes.Keyboard)
                player.Controller = new KeyboardController(_keybindings[i]);
            _players.Add(player);

            AddChild(player);
            player.CurveSpawner.CreatedLine += HandleCreateLine;
            player.GlobalPosition = GetRandomCoordinateInView(100);
            player.PlayerId = playerId;
            i++;
        });
    }

    private bool IsPlayerIntersecting(Player player, ISet<SegmentShape2D> segments)
    {
        var position = player.GlobalPosition;
        var radius = player.GetRadius() + (Constants.LineWidth / 2);

        return segments.Any(
            segment =>
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
    }

    private void HandleCreateLine(Line2D line, SegmentShape2D segment)
    {
        // Draw line to screen
        AddChild(line);

        _grid.AddSegment(segment);
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
