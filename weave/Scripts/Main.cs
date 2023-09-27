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
using weave.MenuControllers;
using weave.Utils;

namespace weave;

internal enum ControllerTypes
{
    Keyboard
}

public partial class Main : Node2D
{
    private Grid _grid;
    private const int NPlayers = 3;
    private const float Acceleration = 3.5f;
    private const int TurnAcceleration = 5;

    private readonly IList<(Key, Key)> _keybindings =
        new List<(Key, Key)>
            { (Key.Left, Key.Right), (Key.Key1, Key.Q), (Key.B, Key.N), (Key.Z, Key.X) };

    private readonly ISet<Player> _players = new HashSet<Player>();
    private ControllerTypes _controllerType = ControllerTypes.Keyboard;

    [GetNode("GameOverOverlay")]
    private GameOverOverlay _gameOverOverlay;

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
        SetupLogger();
    }

    public override void _Process(double delta)
    {
        _players.ForEach(
            p =>
            {
                p.MovementSpeed += Acceleration * (float)delta;
                p.TurnRadius += TurnAcceleration * (float)delta;
            });
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
    }

    private void DetectPlayerCollision()
    {
        var hasCollided = false;

        // Perform collision detection for players that are drawing
        foreach (var player in _players.Where(player => player.CurveSpawner.IsDrawing))
        {
            var segments = _grid.GetSegmentsFromPlayerPosition(
                player.GlobalPosition,
                player.GetRadius()
            );
            if (IsPlayerIntersecting(player, segments))
            {
                hasCollided = true;
            }
        }

        if (hasCollided)
            GameOver();
    }

    private void GameOver()
    {
        _gameOverOverlay.Visible = true;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    private ISet<SegmentShape2D> GetAllSegments()
    {
        return _players.SelectMany(player => player.CurveSpawner.Segments).ToHashSet();
    }

    private void SpawnPlayers()
    {
        NPlayers.TimesDo(
            i =>
            {
                var player = Instanter.Instantiate<Player>();
                player.Color = Unique.NewColor();

                if (_controllerType == ControllerTypes.Keyboard)
                    player.Controller = new KeyboardController(_keybindings[i]);

                AddChild(player);
                player.CurveSpawner.CreatedLine += HandleCreateLine;
                player.GlobalPosition = GetRandomCoordinateInView(100);
                _players.Add(player);
            }
        );
    }

    private static bool IsPlayerIntersecting(Player player, IEnumerable<SegmentShape2D> segments)
    {
        var position = player.CollisionShape2D.GlobalPosition;
        var radius = player.GetRadius() + Constants.LineWidth / 2f;

        return segments.Any(
            segment =>
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
    }

    private void HandleCreateLine(Line2D line, SegmentShape2D segment)
    {
        line.AddToGroup(GroupConstants.LineGroup);
        _grid.AddSegment(segment);
        AddChild(line);
    }

    private void OnPlayerReachedGoal(Player player)
    {
        if (++_roundCompletions != NPlayers)
            return;

        _roundCompletions = 0;
        ClearLinesAndSegments();
        ClearAndSpawnGoals();
    }

    private void ClearLinesAndSegments()
    {
        GetTree().GetNodesInGroup(GroupConstants.LineGroup).ForEach(line => line.QueueFree());

        CreateMapGrid();
    }

    private void ClearAndSpawnGoals()
    {
        // Remove existing goals
        GetTree()
            .GetNodesInGroup(GroupConstants.GoalGroup)
            .ToList()
            .ForEach(goal => goal.QueueFree());

        // Spawn new goals
        _players.ForEach(
            player =>
            {
                var goal = Instanter.Instantiate<Goal>();
                CallDeferred("add_child", goal);
                goal.GlobalPosition = GetRandomCoordinateInView(100);
                goal.PlayerReachedGoal += OnPlayerReachedGoal;
                goal.CallDeferred("set", nameof(Goal.Color), player.Color);
            }
        );
    }

    private Vector2 GetRandomCoordinateInView(float margin)
    {
        var x = (float)GD.RandRange(margin, GetViewportRect().Size.X - margin);
        var y = (float)GD.RandRange(margin, GetViewportRect().Size.Y - margin);
        return new Vector2(x, y);
    }

    private void SetupLogger()
    {
        var logIteration = 0;
        float msFirstLog = -1;

        var loggers = new List<Logger.Logger>()
        {
            // FPS Logger
            new(
                DevConstants.FpsLogFilePath,
                new[] { IdLogger, DeltaLogger, FpsLogger, LineCountLogger }
            ),
            // Speed Logger
            new(
                DevConstants.SpeedLogFilePath,
                new[] { IdLogger,  DeltaLogger, SpeedLogger, TurnRadiusLogger }
            )
        };

        // Log every half second
        AddChild(TimerFactory.StartedRepeating(0.5f, () => loggers.ForEach(l => l.Log())));

        // Save to file every 5 seconds
        AddChild(TimerFactory.StartedRepeating(5f, () => loggers.ForEach(l => l.Persist())));

        // ReSharper disable once SeparateLocalFunctionsWithJumpStatement
        Log FpsLogger()
        {
            var value = Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture);
            return new Log("fps", value);
        }

        Log LineCountLogger()
        {
            var value = GetAllSegments().Count.ToString();
            return new Log("lines", value);
        }

        Log SpeedLogger()
        {
            var value = _players.First().MovementSpeed.ToString(CultureInfo.InvariantCulture);
            return new Log("speed", value);
        }

        Log TurnRadiusLogger()
        {
            var value = _players.First().TurnRadius.ToString(CultureInfo.InvariantCulture);
            return new Log("turn_radius", value);
        }

        Log IdLogger()
        {
            return new Log("id", logIteration++.ToString());
        }

        Log DeltaLogger()
        {
            if (msFirstLog == -1)
                msFirstLog = Time.GetTicksMsec();

            var elapsed = Time.GetTicksMsec() - msFirstLog;

            return new Log("delta_ms", elapsed.ToString());
        }
    }
}