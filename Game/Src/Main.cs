using Godot;
using GodotSharper.AutoGetNode;

public partial class Main : Node2D
{
    [GetNode("Label")]
    private Label _label;

    public override void _Ready()
    {
        this.GetNodes();
        _label.Text = "Hello World!";
        GD.Print("Hello World!");
    }

    public override void _Process(double delta)
    {
    }
}
