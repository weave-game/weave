using Godot;
using GodotSharper.AutoGetNode;

namespace weave.MenuControllers;

public partial class StartScreen : Node2D
{
    private PackedScene _gameScene = GD.Load<PackedScene>("res://Scenes/Main.tscn");

    [GetNode("CanvasLayer/CenterContainer/VBoxContainer/StartButton")]
    private Button _startButton;

    [GetNode("CanvasLayer/CenterContainer/VBoxContainer/QuitButton")]
    private Button _quitButton;
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
