using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;

namespace Weave.MenuControllers;

public partial class GameOverOverlay : CanvasLayer
{
    [GetNode("CenterContainer/VBox/MenuButton")]
    private Button _menuButton;

    [GetNode("CenterContainer/VBox/RetryButton")]
    private Button _retryButton;

    [GetNode("ExplosionPlayer")]
    private AudioStreamPlayer _explosionPlayer;

    public override void _Ready()
    {
        this.GetNodes();
        _retryButton.Pressed += () => GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
        _menuButton.Pressed += () =>
            GetTree().ChangeSceneToFile(SceneGetter.GetPath<StartScreen>());

        // On game over, set process mode to idle to stop game, but keep overlays clickable
        ProcessMode = ProcessModeEnum.Always;
    }

    public void DisplayGameOver()
    {
        _explosionPlayer.Play();
        _retryButton.GrabFocus();
        Show();
    }
}
