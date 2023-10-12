using System;
using System.Threading.Tasks;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using Weave.Utils;

namespace Weave;

public partial class ScoreDisplay : CanvasLayer
{
    private const float PointsForSeconds = 25;
    private const float PointsForRound = 500;
    private const float MinPointsForRound = 150;
    private const float RoundMultiplier = 1.1f;
    private const float PlayerMultiplier = 1.5f;
    private int _finishedRounds;
    private int _playerCount;
    private float _score;

    /// <summary>
    ///     The score. Internally stored as a float to allow for more precise calculations but when used externally its an int.
    /// </summary>
    public int Score => (int)_score;

    [GetNode("CenterContainer/ScoreLabel")]
    private Label _scoreLabel;

    [GetNode("CenterContainer/ScoreLabel/AnimationPlayer")]
    private AnimationPlayer _animationPlayer;

    /// <summary>
    /// Change to switch between different scoring rules
    /// </summary>
    private ScoringRule _scoringRule = ScoringRule.TimeAndRound;

    private double _timeSinceRoundStart;
    private double _points;

    public bool Enabled { set; get; }

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _Process(double delta)
    {
        _scoreLabel.Text = ((int)_score).ToString();

        if (!Enabled)
            return;

        float scoreIncrease = 0;

        switch (_scoringRule)
        {
            case ScoringRule.TimeOnly:
            case ScoringRule.TimeAndRound:
                scoreIncrease += (float)delta * PointsForSeconds;
                break;
            case ScoringRule.TimeOnlyBasedOnRound:
                scoreIncrease +=
                    (float)delta
                    * PointsForSeconds
                    * MathF.Pow(RoundMultiplier, _finishedRounds - 1);
                break;
            case ScoringRule.RoundOnly:
            case ScoringRule.RoundOnlyBasedOnTime:
                break;
            default:
                throw new NotSupportedException($"Unsupported scoring rule: {_scoringRule}");
        }

        _score += scoreIncrease * MathF.Pow(PlayerMultiplier, _playerCount - 1);

        _timeSinceRoundStart += delta;
    }

    public void OnRoundComplete()
    {
        if (!Enabled)
            return;

        float scoreIncrease = 0;

        switch (_scoringRule)
        {
            case ScoringRule.RoundOnly:
            case ScoringRule.TimeAndRound:
                scoreIncrease += PointsForRound;
                break;
            case ScoringRule.RoundOnlyBasedOnTime:
                scoreIncrease += MathF.Max(
                    MinPointsForRound,
                    PointsForRound - ((float)_timeSinceRoundStart * PointsForSeconds)
                );
                break;
            case ScoringRule.TimeOnly:
            case ScoringRule.TimeOnlyBasedOnRound:
                break;
            default:
                throw new NotSupportedException($"Unsupported scoring rule: {_scoringRule}");
        }

        _finishedRounds++;
        _timeSinceRoundStart = 0;

        _animationPlayer.Play(name: "ScoreDisplayShine", customSpeed: 2.0f / WeaveConstants.CountdownLength);

        AddChild(
            TimerFactory.StartedSelfDestructingOneShot(WeaveConstants.CountdownLength / 2.0,
                () => _score += scoreIncrease * MathF.Pow(PlayerMultiplier, _playerCount - 1))
        );
    }

    public void OnGameStart(int playerCount)
    {
        _playerCount = playerCount;
    }

    public void OnGameEnd()
    {
        Enabled = false;
        _animationPlayer.Play("ScoreDisplayEnd");
    }

    private enum ScoringRule
    {
        TimeOnly,
        TimeOnlyBasedOnRound,
        RoundOnly,
        RoundOnlyBasedOnTime,
        TimeAndRound
    }
}