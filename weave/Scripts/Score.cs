using System;
using Godot;
using GodotSharper.AutoGetNode;

namespace weave;

public partial class Score : CanvasLayer
{
    private const double PointsForSeconds = 25;
    private const double PointsForRound = 500;
    private const double MinPointsForRound = 150;
    private const double RoundMultiplier = 1.1;

    private bool _enabled;
    private int _finishedRounds;
    private double _score;

    [GetNode("CenterContainer/ScoreLabel")]
    private Label _scoreLabel;

    // Change to switch between different scoring rules
    private ScoringRule _scoringRule = ScoringRule.TimeOnlyBasedOnRound;
    private double _timeSinceRoundStart;

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _Process(double delta)
    {
        if (!_enabled)
            return;

        switch (_scoringRule)
        {
            case ScoringRule.TimeOnly:
            case ScoringRule.TimeAndRound:
                _score += delta * PointsForSeconds;
                break;
            case ScoringRule.TimeOnlyBasedOnRound:
                _score += delta * PointsForSeconds * Math.Pow(RoundMultiplier, _finishedRounds - 1);
                break;
            case ScoringRule.RoundOnly:
                break;
            case ScoringRule.RoundOnlyBasedOnTime:
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(_scoringRule),
                    "_scoringRule must be a valid ScoringRule"
                );
        }

        _timeSinceRoundStart += delta;
        _scoreLabel.Text = ((int)_score).ToString();
    }

    public void OnRoundComplete()
    {
        if (!_enabled)
            return;

        switch (_scoringRule)
        {
            case ScoringRule.RoundOnly:
            case ScoringRule.TimeAndRound:
                _score += PointsForRound;
                break;
            case ScoringRule.RoundOnlyBasedOnTime:
                _score += Math.Max(
                    MinPointsForRound,
                    PointsForRound - _timeSinceRoundStart * PointsForSeconds
                );
                break;
            case ScoringRule.TimeOnly:
                break;
            case ScoringRule.TimeOnlyBasedOnRound:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _finishedRounds++;
        _timeSinceRoundStart = 0;
    }

    public void OnGameStart()
    {
        _score = 0;
        _finishedRounds = 0;
        _timeSinceRoundStart = 0;
        _enabled = true;
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
