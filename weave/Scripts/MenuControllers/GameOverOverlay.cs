using Godot;
using GodotSharper.AutoGetNode;
using weave.Utils;

namespace weave.MenuControllers;

public partial class GameOverOverlay : CanvasLayer
{
    [GetNode("CenterContainer/VBox/RetryButton")]
    private Button _retryButton;

    [GetNode("CenterContainer/VBox/MenuButton")]
    private Button _menuButton;
    
    public override void _Ready()
    {
        this.GetNodes();
        _retryButton.Pressed += () => GetTree().ChangeSceneToFile(SceneResources.MainScene);
        _menuButton.Pressed += () => GetTree().ChangeSceneToFile(SceneResources.StartScreenScene);

        // On game over, set process mode to idle to stop game, but keep overlays clickable
        ProcessMode = ProcessModeEnum.Always;
    }

    public void FocusRetryButton()
    {
        _retryButton.GrabFocus();
    }
}
