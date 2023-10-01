using Godot;
using System.Collections.Generic;
using System.Linq;

public class BuildingManager
{
    private readonly Main main;
    private readonly Node buildableContainer;

    public BuildingManager(Main main)
    {
        this.main = main;
        buildableContainer = main.GetNode("Buildable");
    }

    public IEnumerable<Buildable> BuildableMachines { get; } = new[]
    {
        // generates power for a full room
        new Buildable("Generator", false,
            "Generator",
            "A generator that can power an additional room", 
            new[] { (ResourceType.BuildingMaterials, 20), (ResourceType.Circuitry, 10), (ResourceType.Tools, 10) }),

        // needed to power up or down a room
        new Buildable("OxygenGenerator", false,
            "Oxygen Sealer",
            "A generator to provide it's room with oxygen",
            new[] { (ResourceType.BuildingMaterials, 2), (ResourceType.DuctTape, 3), (ResourceType.Chemicals, 2) }),
            
        // story goal 1
        new Buildable("ComputerSystem", true,
            "Computer System",
            "The computer system of the station, as long as it is down large parts of the station are locked.",
            new[] { (ResourceType.Circuitry, 15), (ResourceType.DuctTape, 5), (ResourceType.Tools, 15) }),
        
        // story goal 2
        new Buildable("Antenna", true,
            "Comm Antenna",
            "An antenna of the communication system. All antennas must be restored before an emergency signal can be sent.",
            new[] { (ResourceType.Circuitry, 5), (ResourceType.Tools, 5) }),

        // story goal 3 and 4
        new Buildable("FuelPump", true,
            "Fuel Pump",
            "A fuel pump to refuel a landed spacecraft. All pumps need to be restored to refill the escape shuttle.",
            new[] { (ResourceType.BuildingMaterials, 5), (ResourceType.DuctTape, 10), (ResourceType.Tools, 5), (ResourceType.Chemicals, 5) }),
    };

    public void Build(Buildable buildable, Vector2 position)
    {
        var node = InstantiateBuildable(buildable);
        node.GlobalPosition = position;
        node.Visible = true;
        buildableContainer.AddChild(node);
    }

    Node2D InstantiateBuildable(Buildable buildable)
    {
        return main.GetNode<Node2D>($"BuildingPrototypes/{buildable.Key}").Duplicate() as Node2D;
    }

    public class Buildable
    {
        public string Key { get; }
        public bool Repair { get; }
        public string Description { get; }
        public IEnumerable<(ResourceType type, int amount)> Cost { get; }

        public Buildable(string key, bool repair, string name, string description, IEnumerable<(ResourceType type, int amount)> cost)
        {
            Key = key;
            Repair = repair;
            Description = description;
            Cost = cost;
        }
    }
}
