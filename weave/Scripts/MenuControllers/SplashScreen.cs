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

    /// <summary>
    ///     Whether the scene is fading out.
    /// </summary>
    private bool _isFading;

    /// <summary>
    ///    Whether the player has pressed a key at least once, this shows the skip label and allows the player to skip the scene.
    /// </summary>
    private bool _hasPressed;

    /// <summary>
    ///     Whether the player is press the "skip" key.
    ///     Is used to prevent skipping in the first frames since people are spamming keys in the lobby.
    /// </summary>
    private bool _allowInputs;

    private const float MusicFinalPitch = 0.7f;
    private readonly Color _fadeColor = new("1e1e1eff");

    public override void _Ready()
    {
        this.GetNodes();
        _skipLabel.Visible = false;

        AddChild(TimerFactory.StartedOneShot(1, () => _allowInputs = true));
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
        if (!_allowInputs) return;

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
