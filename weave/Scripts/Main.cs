using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.InputSources;
using weave.Logger;
using weave.Logger.Concrete;
using weave.MenuControllers;
using weave.Utils;
using static weave.InputSources.KeyboardBindings;

namespace weave;

public partial class Main : Node2D
{
    private const float Acceleration = 3.5f;
    private const int TurnAcceleration = 5;
    private const int PlayerStartDelay = 2;
    private readonly ISet<Player> _players = new HashSet<Player>();
    private Lobby _lobby = new();

    [GetNode("GameOverOverlay")]
    private GameOverOverlay _gameOverOverlay;

    [GetNode("CountdownLayer/CenterContainer/CountdownLabel")]
    private CountdownLabel _countdownLabel;

    [GetNode("ScoreDisplay")]
    private Score _scoreDisplay;

    /// <summary>
    ///     How many players have reached the goal during the current round.
    /// </summary>
    private int _roundCompletions;

    private Grid _grid;
    private int _width;
    private int _height;
    private Timer _uiUpdateTimer;
    private Timer _playerDelayTimer;

    public override void _Ready()
    {
        this.GetNodes();
        _lobby = GameConfig.Lobby;

        // Fallback to <- and -> if there are no keybindings
        if (_lobby.InputSources.Count == 0)
            _lobby.InputSources.Add(new KeyboardInputSource(Keybindings[0]));

        _width = (int)GetViewportRect().Size.X;
        _height = (int)GetViewportRect().Size.Y;

        InitializeTimers();
        DisablePlayerMovement();
        CreateMapGrid();
        SpawnPlayers();
        ClearAndSpawnGoals();
        SetupLogger();
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
    }

    private void DisablePlayerMovement()
    {
        _players.ForEach(player => player.IsMoving = false);
        _uiUpdateTimer.Timeout += UpdateCountdown;
        _playerDelayTimer.Start();
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
        _playerDelayTimer.Timeout += _scoreDisplay.OnGameStart;
        AddChild(_playerDelayTimer);
    }

    private void UpdateCountdown()
    {
        var newText = Math.Round(_playerDelayTimer.TimeLeft, 1).ToString();
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
            {
                hasCollided = true;
            }
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
            {
                player.Position = new Vector2(_width, pos.Y);
            }
            else if (pos.X > _width)
            {
                player.Position = new Vector2(0, pos.Y);
            }
            else if (pos.Y < 0)
            {
                player.Position = new Vector2(pos.X, _height);
            }
            else if (pos.Y > _height)
            {
                player.Position = new Vector2(pos.X, 0);
            }
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
        List<Vector2> playerPositions = GetRandomPointsInView(_lobby.InputSources.Count);

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
        var radius = player.GetRadius() + Constants.LineWidth / 2f;

        return segments.Any(
            segment =>
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
    }

    private void HandleCreateCollisionLine(Line2D line, SegmentShape2D segment)
    {
        line.AddToGroup(GroupConstants.LineGroup);
        _grid.AddSegment(segment);
        AddChild(line);
    }

    private void OnPlayerReachedGoal(Player player)
    {
        if (++_roundCompletions != _lobby.Count)
            return;

        HandleRoundComplete();
    }

    private void HandleRoundComplete()
    {
        _roundCompletions = 0;

        DisablePlayerMovement();
        ClearLinesAndSegments();
        ClearAndSpawnGoals();
        _scoreDisplay.OnRoundComplete();
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
        List<Vector2> goalPositions = GetRandomPointsInView(_players.Count);
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

    private Vector2 GetRandomCoordinateInView(float margin)
    {
        var x = (float)GD.RandRange(margin, _width - margin);
        var y = (float)GD.RandRange(margin, _height - margin);
        return new Vector2(x, y);
    }

    private List<Vector2> GetRandomPointsInView(int n, float minDistance = 300, float margin = 50) {
        List<Vector2> points = new List<Vector2>();
        if (n < 1) return points;

        const int maxTries = 1000;

        Console.WriteLine($"Generating...");

        // Create the first point anywhere
        points.Add(GetRandomCoordinateInView(margin));
        Console.WriteLine($"Position: {points[0]}");

        // Then generate the rest
        for (int pointsLeft = n - 1; pointsLeft > 0; pointsLeft--) {

            // Pick a random existing point
            Vector2 origo = points[GD.RandRange(0, points.Count - 1)];
            Vector2 newPoint = new Vector2();

            // Generate a random point around it
            bool valid = false;
            for (int tries = 0; tries <= maxTries && !valid; tries++) {
                float radius = (float)GD.RandRange(minDistance, (_width + _height) / 2.0f);
                float angle = (float)GD.RandRange(0, 2 * Math.PI);

                newPoint = new Vector2(
                    origo.X + radius * Mathf.Cos(angle),
                    origo.Y + radius * Mathf.Sin(angle)
                );

                while (origo.X < 0) origo.X += _width;
                while (origo.Y < 0) origo.Y += _height;
                while (origo.X >= _width) origo.X -= _width;
                while (origo.Y >= _height) origo.Y -= _height;

                valid = true;

                // Check if it is far away enough from other points
                foreach (Vector2 point in points) {
                    if (GetDistanceOfPointsInView(newPoint, point) < minDistance) {
                        valid = false;
                        break;
                    }
                }

                // Check if it is outside the margin
                if (margin > newPoint.X || newPoint.X > _width - margin) valid = false;
                if (margin > newPoint.Y || newPoint.Y > _height - margin) valid = false;
            }

            // If generation was not successful, add a random point instead
            if (!valid) {
                Console.WriteLine($"Generating failed from {maxTries} tries, using random coordinates.");
                newPoint = GetRandomCoordinateInView(margin);
            }

            points.Add(newPoint);   

            Console.WriteLine($"Position: {newPoint}");         
        }

        return points;
    }

    private float GetDistanceOfPointsInView(Vector2 a, Vector2 b) {
        float distance = a.DistanceTo(b);

        Vector2 bUp = new Vector2(b.X, b.Y - _height);
        Vector2 bDown = new Vector2(b.X, b.Y + _height);
        Vector2 bLeft = new Vector2(b.X - _width, b.Y);
        Vector2 bRight = new Vector2(b.X + _width, b.Y);

        if (distance > a.DistanceTo(bUp)) distance = a.DistanceTo(bUp);
        if (distance > a.DistanceTo(bDown)) distance = a.DistanceTo(bDown);
        if (distance > a.DistanceTo(bLeft)) distance = a.DistanceTo(bLeft);
        if (distance > a.DistanceTo(bRight)) distance = a.DistanceTo(bRight);

        return distance;
    }

    private void SetupLogger()
    {
        var fpsDeltaLogger = new DeltaLogger();
        var speedDeltaLogger = new DeltaLogger();

        var loggers = new List<Logger.Logger>
        {
            // FPS Logger
            new(
                DevConstants.FpsLogFilePath,
                new[] { () => fpsDeltaLogger.Log(), FpsLogger, LineCountLogger }
            ),
            // Speed Logger
            new(
                DevConstants.SpeedLogFilePath,
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

    #endregion
}
