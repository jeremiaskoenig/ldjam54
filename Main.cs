using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node2D
{
	private TileMap worldMap;
	private TileMap overlayMap;

	public WorldGenerator WorldGenerator  { get; private set; }
	public RoomManager RoomManager { get; private set; }
	public CameraManager CameraManager { get; private set; }
	public ResourceManager ResourceManager { get; private set; }
	public BuildingManager BuildingManager { get; private set; }
	public EnergySystem EnergySystem { get; private set; }
	public EventManager EventManager { get; private set; }
	public AudioManager AudioManager { get; private set; }

	private readonly Dictionary<string, object> configuration = new();
	private readonly Dictionary<string, Room> previousRooms = new();
	private readonly List<Room> changedRooms = new();

	private UserInterface userInterface;

	private Vector2I inactiveTileCoordinates;
	private Vector2I hiddenTileCoordinates;

	public IEnumerable<Character> Characters => GetNode("Characters").GetChildren().OfType<Character>();
	public Node2D SelectedCharacter => Characters.FirstOrDefault(character => character.Selection.IsSelected);

	public Character GetCharacter(string name) => Characters.FirstOrDefault(node => node.Name == name) as Character;

	public T GetConfig<T>(string key)
	{
		return (T)Convert.ChangeType(configuration[key], typeof(T));
	}

	public void RefreshRoom(Room room)
	{
		changedRooms.Add(room);
	}

	private void UpdateRoom(Room room, bool isHidden)
	{
		const int VISIBLE = 0;
		const int INVISIBLE = -1;

		foreach (var (cell, layer) in room.OverlayTiles)
		{
			var overlay = isHidden ? hiddenTileCoordinates : inactiveTileCoordinates;
			int isOverlayVisible = room.IsPowered ? INVISIBLE : VISIBLE;

			var sourceId = worldMap.GetCellSourceId(layer, cell);
			overlayMap.SetCell(2, cell, sourceId, overlay, isOverlayVisible);
		}

		EnergySystem.UpdatePower();

		foreach (Node2D placedObject in GetNode("LootNodes").GetChildren()
															.Concat(GetNode("Buildables").GetChildren()))
		{
			if (RoomManager.GetRoom(placedObject.GlobalPosition) == room)
			{
				placedObject.Visible = room.IsPowered || !isHidden;
			}
		}
	}

	private readonly HashSet<Room> currentRooms = new();
	public override void _PhysicsProcess(double delta)
	{
		currentRooms.Clear();
		foreach (var character in Characters)
		{
			var currentRoom = RoomManager.GetRoom(character.GlobalPosition);
			currentRooms.Add(currentRoom);
			var previousRoom = previousRooms.ContainsKey(character.Name) ? previousRooms[character.Name] : null;
			
			if (currentRoom != previousRoom)
			{
				changedRooms.Add(currentRoom);
				if (previousRoom != null)
				{
					changedRooms.Add(previousRoom);
				}
				previousRooms[character.Name] = currentRoom;

				currentRoom.Trigger();
			}

			if (character.Selection.IsSelected)
			{
				if (CameraManager.GetActiveCamera() != character.Camera && EventManager.GetCameraLock() == false)
				{
					Vector2 defaultier = Vector2.Zero;
					CameraManager.SetActiveCamera(character.Camera);
					CameraManager.SetCameraPosition(defaultier);
					CameraManager.SetCameraLimits();
				}
			}
			else
			{
				if (CameraManager.GetActiveCamera() != GetNode<Camera2D>("WorldCamera") && EventManager.GetCameraLock() == false)
				{
					Vector2 defaultiest = CameraManager.GetActiveCamera().GlobalPosition;
					CameraManager.SetActiveCamera(GetNode<Camera2D>("WorldCamera"));
					CameraManager.SetGlobalCameraPosition(defaultiest);
					CameraManager.SetCameraLimits();
				}
			}
		}

		foreach (var room in changedRooms)
		{
			UpdateRoom(room, room.IsPowered || !currentRooms.Contains(room));
		}
		changedRooms.Clear();

		base._PhysicsProcess(delta);
	}

	public void SpawnCharacter(string characterName, Vector2 position)
	{
		GD.Print($"Spawning character {characterName}@{position}");

		var newCharacter = Characters.First().Duplicate() as Character;
		newCharacter.Name = characterName;
		newCharacter.GlobalPosition = position;
		GetNode("Characters").AddChild(newCharacter);
		newCharacter.Selection.Unselect();
	}

	private int fuelPumpCounter = 0;
	private int antennaCounter = 0;
	internal void TriggerFixed(BuildingManager.Buildable buildable, Vector2 position)
    {
		var buildableContainer = GetNode("Buildables");


		switch (buildable.Key)
        {
			case "ComputerSystem":
                foreach (var door in buildableContainer.GetChildren().OfType<Door>().Where(node => node.Name.ToString().StartsWith("ComputerDoor_")))
                {
					door.IsOpened = true;
                }
				userInterface.UpdateTask("You fixed the computer. Next you need to fix the communication system. The antennas seem broken. The communication array is at the bottom side of the station.");
				break;

			case "Antenna":
				antennaCounter++;
				if (antennaCounter >= 4)
				{
					buildableContainer.GetChildren().OfType<Door>().First(node => node.Name.Equals("FuelDoor")).IsOpened = true;
					userInterface.UpdateTask("You fixed the communication system. Now you only need to get the fuel pumps up and running so you can supply the escape shuttle with fuel to escape. The refinery is somewhere in the center of the station.");
				}
				break;

			case "FuelPump":
				fuelPumpCounter++;
				if (fuelPumpCounter >= 2)
                {
					buildableContainer.GetChildren().OfType<Door>().First(node => node.Name.Equals("EscapeDoor")).IsOpened = true;
					userInterface.UpdateTask("You fixed all nessecary systems to escape! Get to the second docking bay at the right side of the station to escape!");
				}
				break;
        }






        //TODO: do some story stuff
    }

	public override void _Ready()
	{
		RoomManager = new(this);
		CameraManager = new(this);
		ResourceManager = new(this);
		BuildingManager = new(this);
		EnergySystem = new(this);
		EventManager = new(this);
		AudioManager = new(this);

		foreach (string key in GetMetaList())
		{
			if (key.StartsWith("config_"))
			{
				var meta = GetMeta(key);
				switch (meta.VariantType)
				{
					case Variant.Type.Bool:
						configuration[key[7..]] = (bool)meta;
						break;
					case Variant.Type.Int:
						configuration[key[7..]] = (int)meta;
						break;
					case Variant.Type.Float:
						configuration[key[7..]] = (float)meta;
						break;
					case Variant.Type.String:
					case Variant.Type.StringName:
						configuration[key[7..]] = (string)meta;
						break;
					default:
						break;
				}
			}
		}

		inactiveTileCoordinates = (Vector2I)GetMeta("inactiveTile");
		hiddenTileCoordinates = (Vector2I)GetMeta("hiddenTile");


		var roomTemplates = GetNode("RoomTemplates");
		var storyRoomTemplates = GetNode("StoryRooms");
		var lootNodePrototypes = GetNode("LootNodePrototypes");
		var lootNodeContainer = GetNode("LootNodes");
		var startCamera = GetNode<Camera2D>("WorldCamera");
		CameraManager.SetActiveCamera(startCamera);
		CameraManager.SetCameraLimits();
		userInterface = GetNode<UserInterface>("UserInterface");
		worldMap = GetNode<TileMap>("World");
		overlayMap = GetNode<TileMap>("WorldOverlay");
		WorldGenerator = new WorldGenerator(this, worldMap, roomTemplates, storyRoomTemplates, lootNodePrototypes, lootNodeContainer);
		WorldGenerator.InactiveTileCoordinates = inactiveTileCoordinates;
		WorldGenerator.Generate();
		var ParallaxBG = GetNode<ParallaxBackground>("Background").Visible = true;
		changedRooms.AddRange(RoomManager.AllRooms);
	}
}
