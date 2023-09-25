using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.InputHandlers;
using weave.Utils;

namespace weave;

internal enum ControllerTypes
{
    Keyboard
}

public partial class Main : Node2D
{
    private Grid _grid;
    private const int NPlayers = 1;

    private readonly List<(Key, Key)> _keybindings =
        new() { (Key.Left, Key.Right), (Key.Key1, Key.Q), (Key.B, Key.N), (Key.Z, Key.X) };

    private readonly ISet<Player> _players = new HashSet<Player>();
    private ControllerTypes _controllerType = ControllerTypes.Keyboard;

    /// <summary>
    ///     How many players that have reached the goal during the current round.
    /// </summary>
    private int _roundCompletions;

    public override void _Ready()
    {
        this.GetNodes();

        if (_keybindings.Count < NPlayers)
            throw new ArgumentException("More players than available keybindings");

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
        var width = (int)GetViewportRect().Size.X;
        var height = (int)GetViewportRect().Size.Y;
        _grid = new Grid(10, 10, width, height);
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (var i = 0; i < 10; i++)
        {
            for (var j = 0; j < 10; j++)
            {
                DrawRect(_grid._cells[i][j].Rect, new(1,0,0), filled: false);
            }
        }
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
        NPlayers.TimesDo(i =>
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
        });
    }

    private static bool IsPlayerIntersecting(Player player, ISet<SegmentShape2D> segments)
    {
        var position = player.CollisionShape2D.GlobalPosition;
        var radius = player.GetRadius() + (Constants.LineWidth / 2);

        return segments.Any(
            segment =>
                // ReSharper disable once CompareOfFloatsByEqualityOperator
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
        if (++_roundCompletions != NPlayers)
            return;

        _roundCompletions = 0;
        ClearAndSpawnGoals();
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
