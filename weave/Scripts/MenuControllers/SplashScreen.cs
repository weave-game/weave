using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;

namespace Weave.MenuControllers;

[Scene("res://Menus/SplashScreen.tscn")]
public partial class SplashScreen : Node2D
{
    [GetNode("FadeRect")]
    private ColorRect _fadeRect;

    [GetNode("SkipLabel")]
    private RichTextLabel _skipLabel;

    [GetNode("AnimationPlayer")]
    private AnimationPlayer _animationPlayer;

    [GetNode("AudioStreamPlayer")]
    private AudioStreamPlayer _music;

    private bool _isFading;
    private bool _hasPressed;
    private const float MusicFinalPitch = 0.7f;
    private readonly Color _fadeColor = new("1e1e1eff");

    public override void _Ready()
    {
        this.GetNodes();
        _skipLabel.Visible = false;

        AddChild(TimerFactory.StartedOneShot(7, () => _isFading = true));
    }

    public override void _Process(double delta)
    {
        if (!_isFading)
        {
            return;
        }

        _fadeRect.Color = _fadeRect.Color.Lerp(_fadeColor, (float)delta * 1.5f);
        _music.PitchScale = 1 - (_fadeRect.Color.A * MusicFinalPitch);

        if (_fadeRect.Color.A >= 0.99f)
        {
            GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true })
        {
            return;
        }

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
