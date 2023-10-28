using System.Globalization;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using Weave.Utils;

namespace Weave;

public partial class ScoreDisplay : CanvasLayer
{
    private const float PointsPerSeconds = 25;
    private const float PointsPerRound = 500;

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

        // Update UI periodically
        AddChild(
            TimerFactory.StartedRepeating(0.01, () => _scoreLabel.Text = ReadableInteger(Score))
        );
    }

    public override void _Process(double delta)
    {
        if (!Enabled)
            return;

        _score += PointsPerSeconds * (float)delta;
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
            return;

        _finishedRounds++;
        _timeSinceRoundStart = 0;

        _animationPlayer.Play(
            "ScoreDisplayShine",
            customSpeed: 2f / WeaveConstants.CountdownLength
        );

        AddChild(
            TimerFactory.StartedSelfDestructingOneShot(
                WeaveConstants.CountdownLength / 2f,
                () => _score += PointsPerRound
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
