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
    private int _finishedRounds;

    /// <summary>
    ///     The score. Internally stored as a double to allow for more precise calculations but when used externally its an int.
    /// </summary>
    public int Points => (int)_points;

    [GetNode("CenterContainer/ScoreLabel")]
    private Label _scoreLabel;

    // Change to switch between different scoring rules
    private ScoringRule _scoringRule = ScoringRule.TimeOnlyBasedOnRound;

    private double _timeSinceRoundStart;
    private double _points;

    public bool Enabled { set; get; }

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _Process(double delta)
    {
        _scoreLabel.Text = ((int)Points).ToString();

        if (!Enabled)
            return;

        switch (_scoringRule)
        {
            case ScoringRule.TimeOnly:
            case ScoringRule.TimeAndRound:
                _points += delta * PointsForSeconds;
                break;
            case ScoringRule.TimeOnlyBasedOnRound:
                _points +=
                    delta * PointsForSeconds * Math.Pow(RoundMultiplier, _finishedRounds - 1);
                break;
            case ScoringRule.RoundOnly:
                break;
            case ScoringRule.RoundOnlyBasedOnTime:
                break;
            default:
                throw new NotSupportedException($"Unsupported scoring rule: {_scoringRule}");
        }

        _timeSinceRoundStart += delta;
    }

    public void OnRoundComplete()
    {
        if (!Enabled)
            return;

        switch (_scoringRule)
        {
            case ScoringRule.RoundOnly:
            case ScoringRule.TimeAndRound:
                _points += PointsForRound;
                break;
            case ScoringRule.RoundOnlyBasedOnTime:
                _points += Math.Max(
                    MinPointsForRound,
                    PointsForRound - _timeSinceRoundStart * PointsForSeconds
                );
                break;
            case ScoringRule.TimeOnly:
                break;
            case ScoringRule.TimeOnlyBasedOnRound:
                break;
            default:
                throw new NotSupportedException($"Unsupported scoring rule: {_scoringRule}");
        }

        _finishedRounds++;
        _timeSinceRoundStart = 0;
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
