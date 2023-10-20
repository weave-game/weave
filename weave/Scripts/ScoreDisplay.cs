using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using Weave.Scoring;
using Weave.Utils;

namespace Weave;

public partial class ScoreDisplay : CanvasLayer
{
    private const float PointsForSeconds = 25;
    private const float PointsForRound = 500;

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
        _scoreLabel.Text = ((int)_score).ToString();

        if (!Enabled)
        {
            return;
        }

        var scoreIncrease = (float)delta * PointsForSeconds;
        _score += scoreIncrease * _playerCount;

        _timeSinceRoundStart += delta;
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
                () => _score += PointsForRound * _playerCount
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
