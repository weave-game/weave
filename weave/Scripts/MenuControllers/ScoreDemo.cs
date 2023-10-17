using Godot;
using GodotSharper.AutoGetNode;
using Weave.Scoring;
using Weave.Utils;

namespace Weave.MenuControllers;

public partial class ScoreDemo : Node2D
{
    [GetNode("Button")]
    private Button _button;

    [GetNode("LineEdit")]
    private LineEdit _lineEdit;

    private ScoreRecord _score;
    private IScoreManager _scoreManager;

    public override void _Ready()
    {
        this.GetNodes();
        _scoreManager = new JsonScoreManager(WeaveConstants.ScoreLogFileJsonPath);
        _score = new(0, UniqueNameGenerator.Instance.New());
        _lineEdit.Text = _score.Name;

        _button.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        _score.Name = _lineEdit.Text;
        _scoreManager.Save(_score);
    }
}
