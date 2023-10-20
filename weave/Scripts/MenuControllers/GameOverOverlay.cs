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

    [GetNode("NameLineEdit")]
    private LineEdit _nameLineEdit;

    [GetNode("SaveNameButton")]
    private Button _save;

    [GetNode("SavedNameEffect")]
    private CpuParticles2D _savedNameEffect;

    private IScoreManager _scoreManager;
    private Score _score;

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
        _save.Pressed += UpdateTeamName;

        // On game over, set process mode to idle to stop game, but keep overlays clickable
        ProcessMode = ProcessModeEnum.Always;
    }

    private void UpdateTeamName()
    {
        var newName = _nameLineEdit.Text;

        if (string.IsNullOrWhiteSpace(newName))
            return;

        _score.Name = newName;
        _scoreManager.Save(_score);
        _savedNameEffect.Emitting = true;
    }

    public void DisplayGameOver()
    {
        _explosionPlayer.Play();
        _retryButton.GrabFocus();
        Show();
    }

    public void SaveScore(int points)
    {
        if (points <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(points));
        }

        var name = UniqueNameGenerator.Instance.New();
        _score = new Score(points, name);
        _nameLineEdit.Text = name;

        _scoreManager.Save(_score);
    }
}
