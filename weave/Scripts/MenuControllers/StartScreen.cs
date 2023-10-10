using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using weave.Utils;

namespace weave.MenuControllers;

public partial class StartScreen : Node2D
{
    private PackedScene _gameScene = GD.Load<PackedScene>("res://Menus/Lobby.tscn");

    [GetNode("UI/MarginContainer/VBoxContainer/Start")]
    private Button _startButton;

    [GetNode("UI/MarginContainer/VBoxContainer/Options")]
    private Button _optionsButton;

    [GetNode("UI/MarginContainer/VBoxContainer/Quit")]
    private Button _quitButton;

    [GetNode("BlurLayer")] private CanvasLayer _blurLayer;

    private string _startButtonText = "START";
    private string _optionsButtonText = "OPTIONS";
    private string _quitButtonText = "QUIT";

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
        _blurLayer.Visible = true;
        HideButtonText();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void ShowButtonText()
    {
        _startButton.Text = _startButtonText;
        _optionsButton.Text = _optionsButtonText;
        _quitButton.Text = _quitButtonText;
    }

    private void HideButtonText()
    {
        _startButton.Text = "";
        _optionsButton.Text = "";
        _quitButton.Text = "";
    }
}
