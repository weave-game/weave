using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;

namespace Weave.MenuControllers;

public partial class GameOverOverlay : CanvasLayer
{
    [GetNode("CenterContainer/VBox/CenterContainer/VBoxContainer/MenuButton")]
    private Button _menuButton;

    [GetNode("CenterContainer/VBox/CenterContainer/VBoxContainer/RetryButton")]
    private Button _retryButton;

    public override void _Ready()
    {
        this.GetNodes();
        _retryButton.Pressed += () => GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
        _menuButton.Pressed += () =>
            GetTree().ChangeSceneToFile(SceneGetter.GetPath<StartScreen>());

        // On game over, set process mode to idle to stop game, but keep overlays clickable
        ProcessMode = ProcessModeEnum.Always;
    }

    public void FocusRetryButton()
    {
        _retryButton.GrabFocus();
    }
}
