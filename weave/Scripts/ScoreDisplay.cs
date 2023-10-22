using System.Collections.Generic;
using System.Globalization;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using Weave.Utils;

namespace Weave;

public partial class ScoreDisplay : CanvasLayer
{
    private readonly ScoreLogicDelegate _scoreLogicDelegate = new();

    [GetNode("CenterContainer/ScoreLabel/AnimationPlayer")]
    private AnimationPlayer _animationPlayer;

    private int _finishedRounds;
    private int _playerCount;
    private double _points;
    private float _score;

    [GetNode("CenterContainer/ScoreLabel")]
    private Label _scoreLabel;

    private double _timeSinceRoundStart;

    /// <summary>
    ///     The score. Internally stored as a float to allow for more precise calculations but when used externally its an int.
    /// </summary>
    public int Score => (int)_score;

    public bool Enabled { set; get; }

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _Process(double delta)
    {
        _scoreLabel.Text = ReadableInteger(Score);

        if (!Enabled)
        {
            return;
        }

        _score += ScoreLogicDelegate.CalcLinearScore(delta);
        _timeSinceRoundStart += delta;
    }

    /// <summary>
    ///     Example:
    ///     - 1000 -> 1 000
    ///     - 1000000 -> 1 000 000
    /// </summary>
    /// <param name="number">The number to format.</param>
    /// <returns>A readable integer.</returns>
    private static string ReadableInteger(int number)
    {
        var culture = new CultureInfo("en-US"); // To get the correct thousand separator
        return number.ToString("N0", culture).Replace(",", " ");
    }

    public void OnRoundComplete()
    {
        if (!Enabled)
        {
            return;
        }

        _finishedRounds++;
        _timeSinceRoundStart = 0;

        _animationPlayer.Play("ScoreDisplayShine", customSpeed: 2f / WeaveConstants.CountdownLength);

        AddChild(
            TimerFactory.StartedSelfDestructingOneShot(
                WeaveConstants.CountdownLength / 2f,
                () => _score += _scoreLogicDelegate.CalcRoundBonus(_playerCount)
            )
        );
    }

    public void OnGameStart(int playerCount)
    {
        _playerCount = playerCount;
    }

    public void OnGameOver()
    {
        Enabled = false;
        _animationPlayer.Play("ScoreDisplayEnd");
    }
}

public sealed class ScoreLogicDelegate
{
    private const float PointsForSeconds = 1000;
    private const float PointsForRound = 5_000;

    /// <summary>
    ///     All factors are close to 1, so if the player count is not defined, we just use 1.
    /// </summary>
    private const float UndefinedFactor = 1;

    private readonly IDictionary<int, float> OPTIMIZED_SCALING_FACTORS = new Dictionary<int, float>
    {
        { 2, 0.5f }, { 3, 1.5f }, { 4, 1.5f }, { 5, 0.5f }
    };

    private IDictionary<int, float> PlayerScalingFactors { get; } = new Dictionary<int, float>
    {
        { 2, 0.912722f }, { 3, 1.081312f }, { 4, 1.119835f }, { 5, 0.907749f }
    };

    public static float CalcLinearScore(double delta)
    {
        return PointsForSeconds * (float)delta;
    }

    public float CalcRoundBonus(int nPlayers)
    {
        var scalingFactor = OPTIMIZED_SCALING_FACTORS.TryGetValue(nPlayers, out var factor) ? factor : UndefinedFactor;
        return PointsForRound * scalingFactor;
    }
}
