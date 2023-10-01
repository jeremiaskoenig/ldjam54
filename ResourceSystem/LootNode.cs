using Godot;
using System;
using System.Linq;

public partial class LootNode : Node2D
{
    private static readonly ResourceType[] availableResources = Enum.GetValues<ResourceType>();
    public ResourceType Type { get; private set; }
    public int Amount { get; private set; }

    private Main main;
    private Area2D area;
    private Area2D body;
    private bool isSelecting;
    private bool isRightDown;

    public override void _Ready()
    {
        body = GetNode<Area2D>("Body");
        main = GetTree().Root.GetNode<Main>("Main");
        area = main.GetNode<Area2D>("Selection");

        Type = availableResources[GD.RandRange(0, availableResources.Length - 1)];
        Amount = GD.RandRange(3, 5);

        base._Ready();
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
            {
                if (e.IsPressed())
                {
                    isRightDown = true;
                }
                else if (e.IsReleased())
                {
                    isRightDown = false;

                    if (isSelecting && main.Characters.Any(character => CanLoot(character)))
                    {
                        main.ResourceManager.AddResource(Type, Amount);
                        Visible = false;
                        QueueFree();
                    }
                }
            }
        }
        base._Input(e);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isRightDown)
        {
            isSelecting = area.OverlapsArea(body);
        }
        base._PhysicsProcess(delta);
    }

    public bool CanLoot(Node2D character)
    {
        //TODO: Implement the same click detection as in selection
        return GlobalPosition.DistanceTo(character.GlobalPosition) < 5; 
    }
}
