using System;
using Godot;
using GodotSharper.AutoGetNode;

namespace weave;

public partial class ScoreDisplay : CanvasLayer
{
    private const float PointsForSeconds = 25;
    private const float PointsForRound = 500;
    private const float MinPointsForRound = 150;
    private const float RoundMultiplier = 1.1f;
    private const float PlayerMultiplier = 1.5f;
    private int _finishedRounds;
    private int _playerCount;

    public float Score { get; private set; }

    [GetNode("CenterContainer/ScoreLabel")]
    private Label _scoreLabel;

    /// <summary>
    /// Change to switch between different scoring rules
    /// </summary>
    private ScoringRule _scoringRule = ScoringRule.TimeOnlyBasedOnRound;

    private double _timeSinceRoundStart;

    public bool Enabled { set; get; }

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _Process(double delta)
    {
        _scoreLabel.Text = ((int)Score).ToString();

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

        Score += scoreIncrease * MathF.Pow(PlayerMultiplier, _playerCount - 1);

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

        Score += scoreIncrease * MathF.Pow(PlayerMultiplier, _playerCount - 1);

        _finishedRounds++;
        _timeSinceRoundStart = 0;
    }

    public void OnGameStart(int playerCount)
    {
        _playerCount = playerCount;
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