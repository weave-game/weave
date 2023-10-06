using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.Utils;

namespace weave.MenuControllers;

[Scene("res://Menus/StartScreen.tscn")]
public partial class StartScreen : Node2D
{
    private PackedScene _gameScene = GD.Load<PackedScene>("res://Menus/LobbyDemo.tscn");

    [GetNode("CanvasLayer/MarginContainer/VBoxContainer/Quit")]
    private Button _quitButton;

    [GetNode("CanvasLayer/MarginContainer/VBoxContainer/Start")]
    private Button _startButton;

    public override void _Ready()
    {
        this.GetNodes();
        _startButton.Pressed += OnStartButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;

        var colorGen = new UniqueColorGenerator();
        GetTree()
            .GetNodesInGroup(GodotConfig.FireflyGroup)
            .Cast<Firefly>()
            .ForEach(f => f.SetColor(colorGen.NewColor()));
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
