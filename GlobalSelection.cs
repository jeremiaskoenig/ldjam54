using Godot;
using System;

public partial class GlobalSelection : Area2D
{
    public override void _Process(double delta)
    {
        GlobalPosition = GetGlobalMousePosition();
        base._Process(delta);
    }
}
