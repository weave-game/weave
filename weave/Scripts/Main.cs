using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.InputSources;
using Weave.Logging;
using Weave.Logging.ConcreteCsv;
using Weave.MenuControllers;
using Weave.Networking;
using Weave.Scoring;
using Weave.Utils;
using static Weave.InputSources.KeyboardBindings;

namespace Weave;

[Scene("res://Scenes/Main.tscn")]
public partial class Main : Node2D
{
    private readonly ISet<Player> _players = new HashSet<Player>();
    private float _acceleration;

    [GetNode("CountdownLayer/CenterContainer/RoundLabel/AnimationPlayer")]
    private AnimationPlayer _animationPlayer;

    [GetNode("AudioStreamPlayer")]
    private AudioStreamPlayer _audioStreamPlayer;

    [GetNode("Camera")]
    private Camera _camera;

    [GetNode("ClearedLevelPlayer")]
    private AudioStreamPlayer _clearedLevelPlayer;

    private bool _gameIsRunning;

    [GetNode("GameOverOverlay")]
    private GameOverOverlay _gameOverOverlay;

    private Grid _grid;
    private int _height;
    private Lobby _lobby = new();
    private RTCClientManager _multiplayerManager;
    private Timer _playerDelayTimer;

    /// <summary>
    ///     The current round, starts from 1.
    /// </summary>
    private int _round;

    /// <summary>
    ///     How many players have reached the goal during the current round.
    /// </summary>
    private int _roundCompletions;

    [GetNode("CountdownLayer/CenterContainer/RoundLabel")]
    private Label _roundLabel;

    [GetNode("ScoreDisplay")]
    private ScoreDisplay _scoreDisplay;

    private float _turnAcceleration;

    private Timer _uiUpdateTimer;
    private int _width;

    public override void _Ready()
    {
        this.GetNodes();
        _lobby = GameConfig.Lobby;
        _multiplayerManager = GameConfig.MultiplayerManager;
        _multiplayerManager.NotifyStartGameAsync();

        // Fallback to <- and -> if there are no keybindings
        if (_lobby.PlayerInfos.Count == 0)
        {
            _lobby.Join(new KeyboardInputSource(Keybindings[0]));
        }

        _width = (int)GetViewportRect().Size.X;
        _height = (int)GetViewportRect().Size.Y;

        InitializeTimers();
        SpawnPlayers();
        SetPlayerTurning(true);
        SetupLogger();
        StartPreparationPhase();

        _scoreDisplay.OnGameStart(_players.Count);
        _gameIsRunning = true;
    }

    public override void _Process(double delta)
    {
        if (!_gameIsRunning)
        {
            return;
        }

        _players.ForEach(
            p =>
            {
                _acceleration -= 0.002f * _acceleration * (float)delta;
                _turnAcceleration -= 0.002f * _turnAcceleration * (float)delta;

                p.MovementSpeed += _acceleration * (float)delta;
                p.TurnSpeed += _turnAcceleration * (float)delta;
            }
        );
    }

    public override void _Input(InputEvent @event)
    {
        if (!WeaveConstants.DevButtonsEnabled)
        {
            return;
        }

        if (@event is InputEventKey { Keycode: Key.Space, Pressed: true })
        {
            OnPlayerReachedGoal();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_gameIsRunning)
        {
            return;
        }

        DetectPlayerCollision();
        DetectPlayerOutOfBounds();
    }

    private void CreateMapGrid()
    {
        _grid = new(10, 10, _width, _height);
    }

    private void SetPlayerMovement(bool enabled)
    {
        _players.ForEach(player => player.IsMoving = enabled);
    }

    private void SetPlayerTurning(bool enabled)
    {
        _players.ForEach(player => player.IsTurning = enabled);
    }

    private void StartRound()
    {
        SetPlayerMovement(true);
        _uiUpdateTimer.Timeout -= UpdateCountdown;
        _roundLabel.Text = "";
        _scoreDisplay.Enabled = true;
    }

    private void InitializeTimers()
    {
        // Updating UI components
        _uiUpdateTimer = new() { WaitTime = 0.02 };
        AddChild(_uiUpdateTimer);
        _uiUpdateTimer.Start();

        // Countdown timer
        _playerDelayTimer = new() { WaitTime = WeaveConstants.CountdownLength, OneShot = true };
        _playerDelayTimer.Timeout += StartRound;
        AddChild(_playerDelayTimer);
    }

    private void UpdateCountdown()
    {
        _roundLabel.Text = "ROUND " + _round;
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

            if (!IsPlayerIntersecting(player, segments))
            {
                continue;
            }

            GameOver(player.Position);
            break;
        }
    }

    private void DetectPlayerOutOfBounds()
    {
        foreach (var player in _players)
        {
            var pos = player.Position;
            if (pos.X < 0)
            {
                player.Position = new(_width, pos.Y);
            }
            else if (pos.X > _width)
            {
                player.Position = new(0, pos.Y);
            }
            else if (pos.Y < 0)
            {
                player.Position = new(pos.X, _height);
            }
            else if (pos.Y > _height)
            {
                player.Position = new(pos.X, 0);
            }
        }
    }

    private void GameOver(Vector2 collisionPosition)
    {
        _gameIsRunning = false;
        SetPlayerMovement(false);
        SetPlayerTurning(false);
        _scoreDisplay.OnGameOver();
        _camera.OnGameOver(collisionPosition);
        _gameOverOverlay.DisplayGameOver();
        _audioStreamPlayer.PitchScale = 0.5f;
        GameConfig.MultiplayerManager.NotifyEndGameAsync();

        // Log score
        _gameOverOverlay.SaveScore(_scoreDisplay.Score);

        // Log "difficulty"
        var diffLogger = new CsvLogger(
            WeaveConstants.DifficultyLogFileCsvPath,
            new List<Func<Log>>
            {
                () => new("unix_time", Time.GetUnixTimeFromSystem().ToString(CultureInfo.InvariantCulture)),
                () => new("players", _lobby.Count.ToString()),
                () => new("rounds", _round.ToString()),
                () => new("score", _scoreDisplay.Score.ToString()),
                () => new("time_ms", Time.GetTicksMsec().ToString())
            },
            LoggerMode.Append
        );

        diffLogger.Log();
        diffLogger.Persist();
    }

    private ISet<SegmentShape2D> GetAllSegments()
    {
        return _players.SelectMany(player => player.CurveSpawner.Segments).ToHashSet();
    }

    private void SpawnPlayers()
    {
        var playerPositions = GetRandomPositionsInView(_lobby.PlayerInfos.Count);

        _lobby.PlayerInfos.ForEach(
            info =>
            {
                var player = Instanter.Instantiate<Player>();
                player.PlayerInfo = info;

                AddChild(player);
                player.CurveSpawner.CreatedLine += HandleCreateCollisionLine;
                player.GlobalPosition = playerPositions[0];
                playerPositions.RemoveAt(0);
                _players.Add(player);
            }
        );

        // Config speed
        var speed = GameConfig.GetInitialMovementSpeed(_lobby.Count);
        _players.ForEach(p => p.MovementSpeed = speed);

        // Config acceleration
        _acceleration = GameConfig.GetAcceleration(_lobby.Count);
        _turnAcceleration = GameConfig.GetTurnSpeedAcceleration(_lobby.Count);
    }

    private static bool IsPlayerIntersecting(Player player, IEnumerable<SegmentShape2D> segments)
    {
        var position = player.CollisionShape2D.GlobalPosition;
        var radius = player.GetRadius() + (WeaveConstants.LineWidth / 2f);

        return segments.Any(
            segment =>
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
    }

    private void HandleCreateCollisionLine(Line2D line, SegmentShape2D segment)
    {
        line.AddToGroup(WeaveConstants.LineGroup);
        _grid.AddSegment(segment);
        AddChild(line);
    }

    private void OnPlayerReachedGoal()
    {
        if (++_roundCompletions != _lobby.Count)
        {
            return;
        }

        _roundCompletions = 0;
        _clearedLevelPlayer.Play();
        _scoreDisplay.OnRoundComplete();
        StartPreparationPhase();
    }

    private void StartPreparationPhase()
    {
        ResetMap();
        SetPlayerMovement(false);
        _uiUpdateTimer.Timeout += UpdateCountdown;
        _playerDelayTimer.WaitTime = _round == 0 ? WeaveConstants.InitialCountdownLength : WeaveConstants.CountdownLength;
        _playerDelayTimer.Start();
        _scoreDisplay.Enabled = false;

        if (_round == 0)
        {
            AddChild(
                TimerFactory.StartedSelfDestructingOneShot(WeaveConstants.InitialCountdownLength - WeaveConstants.CountdownLength, IncreaseRound)
            );
        }
        else
        {
            IncreaseRound();
        }
    }

    private void IncreaseRound()
    {
        AddChild(
            TimerFactory.StartedSelfDestructingOneShot(WeaveConstants.CountdownLength / 2, () => _round++)
        );

        _animationPlayer.Play("Preparation", customSpeed: 2 / WeaveConstants.CountdownLength);
    }

    private void ClearLinesAndSegments()
    {
        GetTree().GetNodesInGroup(WeaveConstants.LineGroup).ForEach(line => line.QueueFree());

        CreateMapGrid();
    }

    /// <summary>
    ///     Removes all existing goals and obstacles and spawns new ones.
    /// </summary>
    private void ResetMap()
    {
        // --- CLEAR ---
        ClearLinesAndSegments();
        var goals = GetTree().GetNodesInGroup(WeaveConstants.GoalGroup);
        var obstacles = GetTree().GetNodesInGroup(WeaveConstants.ObstacleGroup);
        goals.Union(obstacles).ForEach(node => node.QueueFree());

        // --- SPAWN GOALS ---

        // Generate goal positions
        var goalPositions = GetRandomPositionsInView(_players.Count, _players.Select(p => p.Position).ToList());

        // Spawn new goals
        _players.ForEach(
            player =>
            {
                var goal = Instanter.Instantiate<Goal>();
                CallDeferred("add_child", goal);
                goal.GlobalPosition = goalPositions[0];
                goalPositions.RemoveAt(0);
                goal.PlayerReachedGoal += OnPlayerReachedGoal;
                goal.CallDeferred("set", nameof(Goal.Color), player.PlayerInfo.Color);
                goal.HasLock = true;
                goal.SetPlayerName(player.PlayerInfo.Name);
                goal.UnlockAreaColors = new List<Color> { player.PlayerInfo.Color };
            }
        );

        // --- SPAWN OBSTACLES ---

        // Add all player positions to goal positions:
        var goalsAndPlayers = goalPositions.ToList();
        _players.ForEach(p => goalsAndPlayers.Add(p.Position));
        var obstaclePositions = GetRandomPositionsInView(GameConfig.GetNObstacles(_lobby.Count), goalsAndPlayers);

        obstaclePositions.ForEach(
            position =>
            {
                var obstacle = Instanter.Instantiate<Obstacle>();
                obstacle.AddToGroup(WeaveConstants.ObstacleGroup);

                obstacle.BodyEntered += node =>
                {
                    if (node is not Player)
                    {
                        return;
                    }

                    GameOver(node.Position);
                };

                CallDeferred("add_child", obstacle);
                obstacle.GlobalPosition = position;

                // Random rotation
                obstacle.RotationDegrees = GD.RandRange(0, 360);

                // Random scale
                var area = GD.RandRange(15, 25);
                var side1 = (int)GD.RandRange(1, Math.Sqrt(area));
                var side2 = area / side1;
                obstacle.SetObstacleSize(side1, side2);
            }
        );
    }

    private IList<Vector2> GetRandomPositionsInView(
        int n,
        IReadOnlyList<Vector2> occupiedPositions = null
    )
    {
        const int MaxAttempts = 1000;
        const float MinDistance = 250;
        const float Margin = 100;
        var positions = new List<Vector2>();

        // Generate positions
        for (var i = 0; i < n; i++)
        {
            bool valid;
            var attempt = 0;
            Vector2 newPosition;

            do
            {
                attempt++;

                newPosition = new(
                    (float)GD.RandRange(Margin, _width - Margin),
                    (float)GD.RandRange(Margin, _height - Margin)
                );

                valid = true;

                foreach (var position in occupiedPositions ?? Array.Empty<Vector2>())
                {
                    var distance = position.DistanceTo(newPosition);
                    if (distance < MinDistance)
                    {
                        valid = false;
                    }
                }

                positions.ForEach(
                    position =>
                    {
                        var distance = position.DistanceTo(newPosition);
                        if (distance < MinDistance)
                        {
                            valid = false;
                        }
                    }
                );
            } while (!valid && attempt < MaxAttempts);

            positions.Add(newPosition);
        }

        return positions;
    }

    private void SetupLogger()
    {
        var fpsDeltaLogger = new DeltaLogger();
        var speedDeltaLogger = new DeltaLogger();

        var loggers = new List<ICsvLogger>
        {
            // FPS Logger
            new CsvLogger(
                WeaveConstants.FpsLogFileCsvPath,
                new[] { () => fpsDeltaLogger.Log(), FpsLogger, LineCountLogger },
                LoggerMode.Reset
            ),
            // Speed Logger
            new CsvLogger(
                WeaveConstants.SpeedLogFileCsvPath,
                new[] { () => speedDeltaLogger.Log(), SpeedLogger, TurnRadiusLogger },
                LoggerMode.Reset
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
        return new("fps", Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture));
    }

    private Log LineCountLogger()
    {
        return new("lines", GetAllSegments().Count.ToString());
    }

    private Log SpeedLogger()
    {
        return new(
            "speed",
            _players.First().MovementSpeed.ToString(CultureInfo.InvariantCulture)
        );
    }

    private Log TurnRadiusLogger()
    {
        return new(
            "turn_radius",
            _players.First().TurnSpeed.ToString(CultureInfo.InvariantCulture)
        );
    }

    #endregion Loggers

}
