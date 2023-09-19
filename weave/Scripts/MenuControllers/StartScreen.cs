using Godot;
using GodotSharper.AutoGetNode;

namespace weave.MenuControllers;

public partial class StartScreen : Node2D
{
    [GetNode("CanvasLayer/CenterContainer/VBoxContainer/Button")]
    private Button _startButton;

    private PackedScene _gameScene = GD.Load<PackedScene>("res://Scenes/Main.tscn");

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