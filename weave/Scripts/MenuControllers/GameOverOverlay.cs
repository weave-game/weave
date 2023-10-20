using System;
using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.Scoring;
using Weave.Utils;

namespace Weave.MenuControllers;

public partial class GameOverOverlay : CanvasLayer
{
    [GetNode("ExplosionPlayer")]
    private AudioStreamPlayer _explosionPlayer;

    [GetNode("CenterContainer/VBox/CenterContainer/VBoxContainer/MenuButton")]
    private Button _menuButton;

    [GetNode("CenterContainer/VBox/CenterContainer/VBoxContainer/RetryButton")]
    private Button _retryButton;

    [GetNode("TeamNameLineEdit")]
    private LineEdit _teamNameLineEdit;

    private IScoreManager _scoreManager;

    public override void _Ready()
    {
        this.GetNodes();
        _scoreManager = new JsonScoreManager(WeaveConstants.ScoreLogFileJsonPath);

        _retryButton.Pressed += () => GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
        _menuButton.Pressed += () =>
        {
            GameConfig.MultiplayerManager.StopClientAsync();
            GetTree().ChangeSceneToFile(SceneGetter.GetPath<StartScreen>());
        };

        // On game over, set process mode to idle to stop game, but keep overlays clickable
        ProcessMode = ProcessModeEnum.Always;
    }

    private void UpdateTeamName()
    {
        var newName = _teamNameLineEdit.Text;

        if (string.IsNullOrWhiteSpace(newName))
            return;

        GD.Print("valid");
        _scoreManager.Save();
    }

    public void DisplayGameOver()
    {
        _explosionPlayer.Play();
        _retryButton.GrabFocus();
        Show();
    }

    public void Do(int points)
    {
        if (points <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(points));
        }

        var score = new Score(
            _scoreDisplay.Score,
            UniqueNameGenerator.Instance.New()
        );

        throw new System.NotImplementedException();
    }
}
