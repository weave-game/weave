using Godot;
using GodotSharper.AutoGetNode;

public partial class Main : Node2D
{
    [GetNode("MegaCoolLabel")]
    private Label _label;

    [GetNode("KillArea")]
    private Area2D _area2D;

    [GetNode("Player")]
    private Player _player;

    public override void _Ready()
    {
        this.GetNodes();
        _label.Text = "Hello World!";

        _area2D.BodyEntered += OnAreaEntered;
        _player.PlayerShotBullet += OnPlayerShotBullet;
    }

    private void OnPlayerShotBullet(Node2D bullet, Vector2 globalPosition)
    {
        AddChild(bullet);
        bullet.GlobalPosition = globalPosition;
    }

    private void OnAreaEntered(Node2D node)
    {
        if (node is Player player)
        {
            GD.Print("player entered the area");
            player.QueueFree();
        }
    }
}
