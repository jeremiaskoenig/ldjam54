using Godot;
using System.Collections.Generic;
using System.Linq;

public class BuildingManager
{
    private readonly Main main;
    private readonly Node buildableContainer;
    private readonly List<Vector2> fixedBuildings = new();

    public BuildingManager(Main main)
    {
        this.main = main;
        buildableContainer = main.GetNode("Buildables");
    }

    public IEnumerable<Buildable> Buildables => buildableMachines.Values;

    private readonly Dictionary<string, Buildable> buildableMachines = new()
    {
        { 
            "PowerGenerator",
            // generates power for a full room
            new Buildable("PowerGenerator", false,
                "Generator",
                "A generator that can power an additional room", 
                new[] { (ResourceType.BuildingMaterials, 20), (ResourceType.Circuitry, 10), (ResourceType.Tools, 10) })
        },
        {
            "OxygenGenerator",
            // needed to power up or down a room
            new Buildable("OxygenGenerator", false,
                "Oxygen Sealer",
                "A machine to provide it's room with oxygen",
                new[] { (ResourceType.BuildingMaterials, 2), (ResourceType.DuctTape, 3), (ResourceType.Chemicals, 2) })

        },
        {
            "ComputerSystem",
            // story goal 1
            new Buildable("ComputerSystem", true,
                "Computer System",
                "The computer system of the station, as long as it is down large parts of the station are locked.",
                new[] { (ResourceType.Circuitry, 15), (ResourceType.DuctTape, 5), (ResourceType.Tools, 15) })

        },
        {
            "Antenna",
            // story goal 2
            new Buildable("Antenna", true,
                "Comm Antenna",
                "An antenna of the communication system. All antennas must be restored before an emergency signal can be sent.",
                new[] { (ResourceType.Circuitry, 5), (ResourceType.Tools, 5) })

        },
        {
            "FuelPump",
            // story goal 3 and 4
            new Buildable("FuelPump", true,
                "Fuel Pump",
                "A fuel pump to refuel a landed spacecraft. All pumps need to be restored to refill the escape shuttle.",
                new[] { (ResourceType.BuildingMaterials, 5), (ResourceType.DuctTape, 10), (ResourceType.Tools, 5), (ResourceType.Chemicals, 5) })
        }
    };

    private static readonly string[] repairables = new[]
    {
        "antenna",
        "computer_system",
        "fuel_pump"
    };


    public IEnumerable<Buildable> AvailableBuildables(Vector2 position)
    {
        var tileSize = main.GetConfig<int>("tileSize");
        var room = main.RoomManager.GetRoom(position);
        Vector2I playerPos = new((int)(position.X / tileSize), (int)(position.Y / tileSize));
        if (!room.BuildableWorldMapTiles.Contains(playerPos))
        {
            return Enumerable.Empty<Buildable>();
        }

        List<Buildable> baseBuildables = new()
        {
            buildableMachines["PowerGenerator"],
            buildableMachines["OxygenGenerator"],
        };

        var scaledPos = (playerPos * tileSize) + new Vector2I((int)(tileSize * 0.5f), (int)(tileSize * 0.5f));

        var positionBuildables = buildableContainer.GetChildren()
                                          .OfType<Node2D>()
                                          .Where(buildable => buildable.GlobalPosition == scaledPos);
        var buildable = positionBuildables.FirstOrDefault(buildable => repairables.Contains((string)buildable.SafeGetMeta("buildableType", "")));

        if (buildable != null)
        {
            switch ((string)buildable.SafeGetMeta("buildableType", ""))
            {
                case "antenna":
                    baseBuildables.Add(buildableMachines["Antenna"]);
                    break;
                case "computer_system":
                    baseBuildables.Add(buildableMachines["ComputerSystem"]); 
                    break;
                case "fuel_pump": 
                    baseBuildables.Add(buildableMachines["FuelPump"]);
                    break;
            }
        }

        return baseBuildables;
    }

    public void Spawn(Buildable buildable, Vector2 position)
    {
        var node = InstantiateBuildable(buildable);
        node.GlobalPosition = position;
        node.Visible = true;
        buildableContainer.AddChild(node);
    }

    public void Build(Buildable buildable, Vector2 position)
    {
        if (!main.ResourceManager.CanAfford(buildable.Cost))
            return;

        main.ResourceManager.Pay(buildable.Cost);
        if (buildable.IsRepair)
        {
            fixedBuildings.Add(position);
            DisableSmoke(position);
            main.TriggerFixed(buildable, position);
        }
        else
        {
            Spawn(buildable, position);
        }
    }

    private void DisableSmoke(Vector2 position)
    {
        var buildable = buildableContainer.GetChildren().OfType<Node2D>().FirstOrDefault(buildable => buildable.GlobalPosition == position);

        if (buildable != null)
        {
            var particles = buildable.GetNode<GpuParticles2D>("BrokenParticles");
            particles.Emitting = false;
            particles.QueueFree();
        }
        else
        {
            GD.PushWarning($"tried to disable smoke for object at {position} but no object found");
        }
    }

    Node2D InstantiateBuildable(Buildable buildable)
    {
        return main.GetNode<Node2D>($"BuildingPrototypes/{buildable.Key}").Duplicate() as Node2D;
    }

    public class Buildable
    {
        public string Key { get; }
        public bool IsRepair { get; }
        public string Name { get; }
        public string Description { get; }
        public IEnumerable<(ResourceType type, int amount)> Cost { get; }

        public Buildable(string key, bool repair, string name, string description, IEnumerable<(ResourceType type, int amount)> cost)
        {
            Key = key;
            IsRepair = repair;
            Name = name;
            Description = description;
            Cost = cost;
        }

        public override string ToString()
        {
            return $"#{Key}; {Name}; {(IsRepair ? "repair" : "")}";
        }
    }

    public bool IsClear(Vector2I playerPos)
    {
        var tileSize = main.GetConfig<int>("tileSize");
        var scaledPos = (playerPos * tileSize) + new Vector2I((int)(tileSize * 0.5f), (int)(tileSize * 0.5f));
        
        return !buildableContainer.GetChildren().OfType<Node2D>().Any(buildable => buildable.GlobalPosition == scaledPos);
    }

    public bool IsType(Vector2I playerPos, string type)
    {
        var tileSize = main.GetConfig<int>("tileSize");
        var scaledPos = (playerPos * tileSize) + new Vector2I((int)(tileSize * 0.5f), (int)(tileSize * 0.5f));

        return buildableContainer.GetChildren()
                                 .OfType<Node2D>()
                                 .Where(buildable => (string)buildable.GetMeta("buildableType") == type)
                                 .Any(buildable => buildable.GlobalPosition == scaledPos);
    }

    public bool IsFixed(Vector2I playerPos)
    {
        var tileSize = main.GetConfig<int>("tileSize");
        var scaledPos = (playerPos * tileSize) + new Vector2I((int)(tileSize * 0.5f), (int)(tileSize * 0.5f));
        return fixedBuildings.Contains(scaledPos);
    }
}
