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
		TIME_ONLY_BASED_ON_LEVEL,
		LEVEL_ONLY,
		LEVEL_ONLY_BASED_ON_TIME,
		TIME_AND_LEVEL,
	}

	private double _score = 0;
	private double _timeSinceLevelStart = 0;
	private int _level = 1;
	private const double _scorePerSecond = 25;
	private const double _scorePerLevel = 1000;

	private ScoringRule _scoringRule = ScoringRule.TIME_ONLY_BASED_ON_LEVEL; // << Change

	[GetNode("CenterContainer/ScoreLabel")]
	private Label _scoreLabel; 

	public override void _Ready()
	{
		this.GetNodes();
	}

	public override void _Process(double delta)
	{
		switch (_scoringRule)
		{
			case ScoringRule.TIME_ONLY:
			case ScoringRule.TIME_AND_LEVEL:
				_score += delta * ScoringConstants.PointsForSeconds;
				break;
			case ScoringRule.TIME_ONLY_BASED_ON_LEVEL:
				_score += delta * ScoringConstants.PointsForSeconds * Math.Pow(ScoringConstants.LevelMultiplier, _level - 1);
				break;
			default:
				break;
		}

		_timeSinceLevelStart += delta;
		_scoreLabel.Text = ((int)_score).ToString();
	}

	public void OnLevelUp(int currentLevel)
	{
		_level = currentLevel;

		switch (_scoringRule)
		{
			case ScoringRule.LEVEL_ONLY:
			case ScoringRule.TIME_AND_LEVEL:
				_score += ScoringConstants.PointsForLevelUp;
				break;
			case ScoringRule.LEVEL_ONLY_BASED_ON_TIME:
				_score += Math.Max(ScoringConstants.MinPointsForLevelUp, ScoringConstants.PointsForLevelUp - _timeSinceLevelStart * ScoringConstants.PointsForSeconds);
				break;
			default:
				break;
		}

		_timeSinceLevelStart = 0;
	}
}