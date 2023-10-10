using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.InputSources;
using Weave.Logger;
using Weave.Logger.Concrete;
using Weave.MenuControllers;
using Weave.Utils;
using static Weave.InputSources.KeyboardBindings;

namespace Weave;

[Scene("res://Scenes/Main.tscn")]
public partial class Main : Node2D
{
    private const float Acceleration = 3.5f;
    private const int TurnAcceleration = 5;
    private const int PlayerStartDelay = 2;
    private readonly ISet<Player> _players = new HashSet<Player>();

    [GetNode("CountdownLayer/CenterContainer/CountdownLabel")]
    private CountdownLabel _countdownLabel;

    [GetNode("GameOverOverlay")]
    private GameOverOverlay _gameOverOverlay;

    private Grid _grid;
    private int _height;
    private Lobby _lobby = new();
    private Timer _playerDelayTimer;

    /// <summary>
    ///     How many players have reached the goal during the current round.
    /// </summary>
    private int _roundCompletions;

    [GetNode("ScoreDisplay")]
    private ScoreDisplay _scoreDisplay;

    private Timer _uiUpdateTimer;
    private int _width;

    public override void _Ready()
    {
        this.GetNodes();
        _lobby = GameConfig.Lobby;

        // Fallback to <- and -> if there are no keybindings
        if (_lobby.InputSources.Count == 0)
            _lobby.Join(new KeyboardInputSource(Keybindings[0]));

        _width = (int)GetViewportRect().Size.X;
        _height = (int)GetViewportRect().Size.Y;

        InitializeTimers();
        DisablePlayerMovement();
        CreateMapGrid();
        SpawnPlayers();
        ClearAndSpawnGoals();
        SetupLogger();

        _scoreDisplay.OnGameStart(_players.Count);
    }

    public override void _Process(double delta)
    {
        _players.ForEach(p =>
        {
            p.MovementSpeed += Acceleration * (float)delta;
            p.TurnRadius += TurnAcceleration * (float)delta;
        });
    }

    public override void _PhysicsProcess(double delta)
    {
        DetectPlayerCollision();
        DetectPlayerOutOfBounds();
    }

    private void CreateMapGrid()
    {
        _grid = new Grid(10, 10, _width, _height);
    }

    private void EnablePlayerMovement()
    {
        _players.ForEach(player => player.IsMoving = true);
        _uiUpdateTimer.Timeout -= UpdateCountdown;
        _countdownLabel.UpdateLabelText("");
        _scoreDisplay.Enabled = true;
    }

    private void DisablePlayerMovement()
    {
        _players.ForEach(player => player.IsMoving = false);
        _uiUpdateTimer.Timeout += UpdateCountdown;
        _playerDelayTimer.Start();
        _scoreDisplay.Enabled = false;
    }

    private void InitializeTimers()
    {
        // Updating UI components
        _uiUpdateTimer = new Timer { WaitTime = 0.1 };
        AddChild(_uiUpdateTimer);
        _uiUpdateTimer.Start();

        // Countdown timer
        _playerDelayTimer = new Timer { WaitTime = PlayerStartDelay, OneShot = true };
        _playerDelayTimer.Timeout += EnablePlayerMovement;
        AddChild(_playerDelayTimer);
    }

    private void UpdateCountdown()
    {
        var newText = Math.Round(_playerDelayTimer.TimeLeft, 1)
            .ToString(CultureInfo.InvariantCulture);
        _countdownLabel.UpdateLabelText(newText);
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
                hasCollided = true;
        }

        if (hasCollided)
            GameOver();
    }

    private void DetectPlayerOutOfBounds()
    {
        foreach (var player in _players)
        {
            var pos = player.Position;
            if (pos.X < 0)
                player.Position = new Vector2(_width, pos.Y);
            else if (pos.X > _width)
                player.Position = new Vector2(0, pos.Y);
            else if (pos.Y < 0)
                player.Position = new Vector2(pos.X, _height);
            else if (pos.Y > _height)
                player.Position = new Vector2(pos.X, 0);
        }
    }

    private void GameOver()
    {
        _gameOverOverlay.Visible = true;
        _gameOverOverlay.FocusRetryButton();
        ProcessMode = ProcessModeEnum.Disabled;
    }

    private ISet<SegmentShape2D> GetAllSegments()
    {
        return _players.SelectMany(player => player.CurveSpawner.Segments).ToHashSet();
    }

    private void SpawnPlayers()
    {
        var colorGenerator = new UniqueColorGenerator();

        var playerPositions = GetRandomPositionsInView(_lobby.InputSources.Count);

        _lobby.InputSources.ForEach(input =>
        {
            var player = Instanter.Instantiate<Player>();
            player.Color = colorGenerator.NewColor();
            player.InputSource = input;

            AddChild(player);
            player.CurveSpawner.CreatedLine += HandleCreateCollisionLine;
            player.GlobalPosition = playerPositions[0];
            playerPositions.RemoveAt(0);
            _players.Add(player);
        });
    }

    private static bool IsPlayerIntersecting(Player player, IEnumerable<SegmentShape2D> segments)
    {
        var position = player.CollisionShape2D.GlobalPosition;
        var radius = player.GetRadius() + (Constants.LineWidth / 2f);

        return segments.Any(
            segment =>
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
    }

    private void HandleCreateCollisionLine(Line2D line, SegmentShape2D segment)
    {
        line.AddToGroup(GodotConfig.LineGroup);
        _grid.AddSegment(segment);
        AddChild(line);
    }

    private void OnPlayerReachedGoal()
    {
        if (++_roundCompletions != _lobby.Count)
            return;

        HandleRoundComplete();
    }

    private void HandleRoundComplete()
    {
        _roundCompletions = 0;

        _scoreDisplay.OnRoundComplete();

        DisablePlayerMovement();
        ClearLinesAndSegments();
        ClearAndSpawnGoals();
    }

    private void ClearLinesAndSegments()
    {
        GetTree().GetNodesInGroup(GodotConfig.LineGroup).ForEach(line => line.QueueFree());

        CreateMapGrid();
    }

    private void ClearAndSpawnGoals()
    {
        // Remove existing goals
        GetTree()
            .GetNodesInGroup(GodotConfig.GoalGroup)
            .ToList()
            .ForEach(goal => goal.QueueFree());

        // Generate goal positions
        IList<Vector2> playerPositions = new List<Vector2>();
        _players.ForEach(player => playerPositions.Add(player.Position));
        var goalPositions = GetRandomPositionsInView(_players.Count, playerPositions);

        // Spawn new goals
        _players.ForEach(player =>
        {
            var goal = Instanter.Instantiate<Goal>();
            CallDeferred("add_child", goal);
            goal.GlobalPosition = goalPositions[0];
            goalPositions.RemoveAt(0);
            goal.PlayerReachedGoal += OnPlayerReachedGoal;
            goal.CallDeferred("set", nameof(Goal.Color), player.Color);
        });
    }

    private IList<Vector2> GetRandomPositionsInView(
        int n,
        IList<Vector2> occupiedPositions = null,
        float minDistance = 250,
        float margin = 100
    )
    {
        var positions = new List<Vector2>();
        const int MaxAttempts = 1000;

        // Generate positions
        for (var i = 0; i < n; i++)
        {
            var valid = false;
            var attempt = 0;
            Vector2 newPosition;

            do
            {
                attempt++;

                newPosition = new Vector2(
                    (float)GD.RandRange(margin, _width - margin),
                    (float)GD.RandRange(margin, _height - margin)
                );

                valid = true;

                occupiedPositions?.ForEach(position =>
                {
                    var distance = position.DistanceTo(newPosition);
                    if (distance < minDistance)
                        valid = false;
                });

                positions.ForEach(position =>
                {
                    var distance = position.DistanceTo(newPosition);
                    if (distance < minDistance)
                        valid = false;
                });
            } while (!valid && attempt < MaxAttempts);

            positions.Add(newPosition);
        }

        return positions;
    }

    private void SetupLogger()
    {
        var fpsDeltaLogger = new DeltaLogger();
        var speedDeltaLogger = new DeltaLogger();

        var loggers = new List<Logger.Logger>
        {
            // FPS Logger
            new(
                Constants.FpsLogFilePath,
                new[] { () => fpsDeltaLogger.Log(), FpsLogger, LineCountLogger }
            ),
            // Speed Logger
            new(
                Constants.SpeedLogFilePath,
                new[] { () => speedDeltaLogger.Log(), SpeedLogger, TurnRadiusLogger }
            )
        };

        // Log every half second
        AddChild(TimerFactory.StartedRepeating(0.5f, () => loggers.ForEach(l => l.Log())));

        // Save to file every 5 seconds
        AddChild(TimerFactory.StartedRepeating(5f, () => loggers.ForEach(l => l.Persist())));
    }

    #region Loggers

    private static Log FpsLogger()
    {
        return new Log("fps", Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture));
    }

    private Log LineCountLogger()
    {
        return new Log("lines", GetAllSegments().Count.ToString());
    }

    private Log SpeedLogger()
    {
        return new Log(
            "speed",
            _players.First().MovementSpeed.ToString(CultureInfo.InvariantCulture)
        );
    }

    private Log TurnRadiusLogger()
    {
        return new Log(
            "turn_radius",
            _players.First().TurnRadius.ToString(CultureInfo.InvariantCulture)
        );
    }

    #endregion Loggers
}
