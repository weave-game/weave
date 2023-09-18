using Godot;
using System;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GD.Print("Hello World!");
    }

    public override void _Process(double delta)
    {
    }
}
