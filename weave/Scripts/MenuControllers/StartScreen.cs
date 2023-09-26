using Godot;
using GodotSharper.AutoGetNode;
using weave.Utils;

namespace weave.MenuControllers;

public partial class StartScreen : Node2D
{
    private PackedScene _gameScene = GD.Load<PackedScene>(SceneResources.MainScene);

    [GetNode("CanvasLayer/CenterContainer/VBoxContainer/QuitButton")]
    private Button _quitButton;

    [GetNode("CanvasLayer/CenterContainer/VBoxContainer/StartButton")]
    private Button _startButton;

    [GetNode("FireflyLayer")]
    private CanvasLayer _fireflyLayer;

    public override void _Ready()
    {
        this.GetNodes();
        _startButton.Pressed += OnStartButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
    }

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToPacked(_gameScene);
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
