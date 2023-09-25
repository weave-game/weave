using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.InputHandlers;
using weave.Logger;
using weave.Utils;

namespace weave;

internal enum ControllerTypes
{
    Keyboard
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
        SetupLogger();
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
        _players
            .Where(p => p.CurveSpawner.IsDrawing)
            .Where(p => IsIntersecting(p, allSegments))
            .ForEach(p => GD.Print($"Player {p.PlayerId} has collided"));
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
            player.CurveSpawner.CreatedLine += line => AddChild(line);
            player.GlobalPosition = GetRandomCoordinateInView(100);
            player.PlayerId = playerId;
        });
    }

    private static bool IsIntersecting(Player player, IEnumerable<SegmentShape2D> segments)
    {
        var position = player.CollisionShape2D.GlobalPosition;
        var radius = player.CircleShape.Radius + Constants.LineWidth / 2f;

        return segments.Any(
            segment =>
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
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

    private void SetupLogger()
    {
        var fpsLogger = () =>
        {
            var value = Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture);
            return new Log("fps", value);
        };

        var logger = new Logger.Logger("log.csv", new[] { fpsLogger });

        AddChild(
            TimerFactory.StartedRepeating(1, () => logger.Log())
        );

        AddChild(
            TimerFactory.StartedRepeating(1.5f, () => logger.Persist())
        );
    }
}