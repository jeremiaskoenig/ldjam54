using Godot;
using System.Collections.Generic;
using System.Linq;

public class BuildingManager
{
    private readonly Main main;

    public BuildingManager(Main main)
    {
        this.main = main;
        Build();
    }

    public IEnumerable<Buildable> BuildableMachines { get; } = new[]
    {
        // Generates power for a full room
        new Buildable("Generator", false, "", new[] { (ResourceType.BuildingMaterials, 20), (ResourceType.DuctTape, 10), (ResourceType.Tools, 10) }),

        // Needed to power up a room
        new Buildable("OxygenGenerator", false, "", new[] { (ResourceType.BuildingMaterials, 2), (ResourceType.DuctTape, 3), (ResourceType.Tools, 2) }),
            
        // Can be used to power up/down a room
        new Buildable("PowerSwitch", false, "", new[] { (ResourceType.Circuitry, 3), (ResourceType.Tools, 2) }),

        new Buildable("ComputerSystem", true, "", new[] { (ResourceType.Circuitry, 15), (ResourceType.DuctTape, 5), (ResourceType.Tools, 15) }),
        new Buildable("Antenna", true, "", new[] { (ResourceType.Circuitry, 10), (ResourceType.Tools, 5) }),
        new Buildable("FuelPump", true, "", new[] { (ResourceType.BuildingMaterials, 15), (ResourceType.DuctTape, 10), (ResourceType.Tools, 5), (ResourceType.Chemicals, 20) }),
    };

    void Build()
    {
        var node = InstantiateBuildable(BuildableMachines.ToArray()[0]);
        node.GlobalPosition = new Vector2(48, 48);
        node.Visible = true;
        main.AddChild(node);
    }

    Node2D InstantiateBuildable(Buildable buildable)
    {
        return main.GetNode<Node2D>($"BuildingPrototypes/{buildable.Name}").Duplicate() as Node2D;
    }

    public class Buildable
    {
        public string Name { get; }
        public bool Repair { get; }

        public Buildable(string name, bool repair, string description, IEnumerable<(ResourceType type, int amount)> cost)
        {
            Name = name;
            Repair = repair;
        }
    }
}
