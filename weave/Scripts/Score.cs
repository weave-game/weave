using Godot;
using GodotSharper.AutoGetNode;
using System;
using weave.Utils;

namespace weave;

public partial class Score : CanvasLayer
{
    private enum ScoringRule
    {
        TIME_ONLY,
        TIME_ONLY_BASED_ON_ROUND,
        ROUND_ONLY,
        ROUND_ONLY_BASED_ON_TIME,
        TIME_AND_ROUND,
    }

    // Change to switch between different scoring rules
    private ScoringRule _scoringRule = ScoringRule.TIME_ONLY_BASED_ON_ROUND;

    public bool Enabled {
        set { _enabled = value; }
        get { return _enabled; }
    }

    private bool _enabled = false;
    private double _score = 0;
    private double _timeSinceRoundStart = 0;
    private int _finishedRounds = 0;

    private const double PointsForSeconds = 25;
    private const double PointsForRound = 500;
    private const double MinPointsForRound = 150;
    private const double RoundMultiplier = 1.1;

    [GetNode("CenterContainer/ScoreLabel")]
    private Label _scoreLabel;

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _Process(double delta)
    {
        _scoreLabel.Text = ((int)_score).ToString();

        if (!_enabled)
            return;

        switch (_scoringRule)
        {
            case ScoringRule.TIME_ONLY:
            case ScoringRule.TIME_AND_ROUND:
                _score += delta * PointsForSeconds;
                break;
            case ScoringRule.TIME_ONLY_BASED_ON_ROUND:
                _score += delta * PointsForSeconds * Math.Pow(RoundMultiplier, _finishedRounds - 1);
                break;
            default:
                break;
        }

        _timeSinceRoundStart += delta;
    }

    public void OnRoundComplete()
    {
        if (!_enabled)
            return;

        switch (_scoringRule)
        {
            case ScoringRule.ROUND_ONLY:
            case ScoringRule.TIME_AND_ROUND:
                _score += PointsForRound;
                break;
            case ScoringRule.ROUND_ONLY_BASED_ON_TIME:
                _score += Math.Max(
                    MinPointsForRound,
                    PointsForRound - _timeSinceRoundStart * PointsForSeconds
                );
                break;
            default:
                break;
        }

        _finishedRounds++;
        _timeSinceRoundStart = 0;
    }
}
