using Godot;
using System;

public partial class CharacterSelection : Node2D
{
    private bool isLeftDown = false;
    private bool isSelecting = false;
    private Area2D area;
    private Area2D body;
    private Node2D selection;
    public bool IsSelected { get; private set; }

    public override void _Ready()
    {
        var parent = GetParent<Node2D>();
        selection = GetNode<Node2D>("SelectionCircle");
        area = parent.GetNode<Area2D>("MouseOverArea");
        body = parent.GetNode<Area2D>("Body");
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
                    IsSelected = isSelecting;
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