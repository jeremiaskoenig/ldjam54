using Godot;
using System;

public partial class Character : Node2D
{
    private bool isLeftDown = false;
    private bool isSelecting = false;
    private Area2D area;
    private Area2D body;
    private Node2D selection;

    public override void _Ready()
    {
        selection = GetNode<Node2D>("Selection");
        area = GetNode<Area2D>("MouseOverArea");
        body = GetNode<Area2D>("Body");

        GD.Print(area);
        GD.Print(body);
        base._Ready();
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                if (e.IsPressed())
                {
                    isLeftDown = true;
                }
                else if (e.IsReleased())
                {
                    isLeftDown = false;
                    selection.Visible = isSelecting;
                    if (isSelecting)
                    {
                        GD.Print("Character selected");
                    }
                    else
                    {
                        GD.Print("Character unselected");
                    }
                }
            }
        }

        base._Input(e);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isLeftDown)
        {
            Vector2 mousePosition = GetViewport().GetMousePosition();
            area.GlobalPosition = mousePosition;
            isSelecting = area.OverlapsArea(body);
        }
        base._PhysicsProcess(delta);
    }
}
