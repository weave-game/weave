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

        SpawnPlayers();
        ClearAndSpawnGoals();
    }

    public override void _PhysicsProcess(double delta)
    {
        DetectPlayerCollision();
    }

    private void DetectPlayerCollision()
    {
        // Collect all segments
        var allSegments = _players.SelectMany(player => player.CurveSpawner.Segments).ToHashSet();

        // Perform collision detection for all players that are drawing
        // Players that are not drawing should not be able to collide
        var drawingPlayers = _players.Where(player => player.CurveSpawner.IsDrawing);
        foreach (var player in drawingPlayers)
            if (IsIntersecting(player, allSegments))
                GD.Print("Player has collided");
    }

    private void SpawnPlayers()
    {
        var i = 0;
        NPlayers.TimesDo(() =>
        {
            var playerId = UniqueId.Generate();
            var player = Instanter.Instantiate<Player>();
            if (_controllerType == ControllerTypes.Keyboard)
                player.Controller = new KeyboardController(_keybindings[i]);
            _players.Add(player);

            AddChild(player);
            player.CurveSpawner.CreatedLine += HandleDrawLine;
            player.GlobalPosition = GetRandomCoordinateInView(100);
            player.PlayerId = playerId;
            i++;
        });
    }

    private static bool IsIntersecting(Player player, IEnumerable<SegmentShape2D> segments)
    {
        var position = player.GlobalPosition;
        var circleShape = (CircleShape2D)player.CollisionShape2D.Shape;
        var radius = circleShape.Radius + Constants.LineWidth / 2f;

        return segments.Any(
            segment =>
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
    }

    private void HandleDrawLine(Line2D line)
    {
        AddChild(line);
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
