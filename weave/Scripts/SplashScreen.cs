using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;

namespace Weave;

[Scene("res://Menus/SplashScreen.tscn")]
public partial class SplashScreen : Node2D
{
    [GetNode("FadeRect")]
    private ColorRect _fadeRect;

    [GetNode("SkipLabel")]
    private RichTextLabel _skipLabel;

    [GetNode("AnimationPlayer")]
    private AnimationPlayer _animationPlayer;

    private bool _isFading;
    private bool _hasPressed;

    public override void _Ready()
    {
        this.GetNodes();
        _skipLabel.Visible = false;

        Timer timer = new() { WaitTime = 5, OneShot = true};
        AddChild(timer);
        timer.Timeout += () => _isFading = true;
        timer.Start();
    }

    public override void _Process(double delta)
    {
        if (_isFading)
        {
            _fadeRect.Color = _fadeRect.Color.Lerp(new Color("1e1e1eff"), (float)delta * 5f);

            if (_fadeRect.Color.A >= 0.99f)
            {
                GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true })
        {
            if (_hasPressed)
            {
                _isFading = true;
            }
            else
            {
                _hasPressed = true;
                _animationPlayer.Play("SkipLabel");
            }
        }
    }
}