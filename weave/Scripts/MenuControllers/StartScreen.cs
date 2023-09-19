using Godot;
using GodotSharper.AutoGetNode;

namespace weave.MenuControllers;

public partial class StartScreen : Node2D
{
    private PackedScene _gameScene = GD.Load<PackedScene>("res://Scenes/Main.tscn");

    [GetNode("CanvasLayer/CenterContainer/VBoxContainer/Button")]
    private Button _startButton;

    public override void _Ready()
    {
        this.GetNodes();
        _startButton.Pressed += OnStartButtonPressed;
    }

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToPacked(_gameScene);
    }
}
