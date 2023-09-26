using System.Linq;
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

    [GetNode("FlyingLightLayer")]
    private CanvasLayer _flyingLightLayer;

    [GetNode("FlyingLightLayer/FlyingLightPath/PathFollow2D/FlyingLight")]
    private FlyingLight _flyingLight;

    private Line2D test;

    public override void _Ready()
    {
        this.GetNodes();
        _flyingLightLayer.AddChild(test);
        _startButton.Pressed += OnStartButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
        _flyingLight.CreatePath += OnCreateLine;
    }

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToPacked(_gameScene);
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnCreateLine(Vector2[] points)
    {
    }
}
