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

	private readonly Dictionary<string, object> configuration = new();

	private Vector2I inactiveTileCoordinates;
	private Vector2I hiddenTileCoordinates;

	private Room previousRoom;
	private readonly List<Room> changedRooms = new();

	public IEnumerable<Node2D> Characters => GetNode("Characters").GetChildren().OfType<Node2D>();

	public T GetConfig<T>(string key)
	{
		return (T)Convert.ChangeType(configuration[key], typeof(T));
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
			{
				if (e.IsReleased())
				{
					var room = RoomManager.GetRoom(GetGlobalMousePosition());
					room.IsPowered = !room.IsPowered;
					changedRooms.Add(room);
				}
			}
		}
		base._Input(e);
	}

	private void UpdateRoom(Room room, bool isHidden)
	{
		const int VISIBLE = 0;
		const int INVISIBLE = -1;

		foreach (var cell in room.WorldMapTiles)
		{
			var overlay = isHidden ? hiddenTileCoordinates : inactiveTileCoordinates;
			int isOverlayVisible = room.IsPowered ? INVISIBLE : VISIBLE;

			var sourceId = worldMap.GetCellSourceId(0, cell);
			overlayMap.SetCell(2, cell, sourceId, overlay, isOverlayVisible);
		}

		foreach (Node2D lootNode in GetNode("LootNodes").GetChildren())
		{
			if (RoomManager.GetRoom(lootNode.GlobalPosition) == room)
			{
				lootNode.Visible = !isHidden;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		var character = GetNode<Node2D>("Characters/Character");
		var currentRoom = RoomManager.GetRoom(character.GlobalPosition);
		

		if (currentRoom != previousRoom)
		{
			changedRooms.Add(currentRoom);
			if (previousRoom != null)
			{
				changedRooms.Add(previousRoom);
			}
			previousRoom = currentRoom;
		}

		if(character.GetNode<CharacterSelection>("Selection").IsSelected)
		{
			if(CameraManager.GetActiveCamera() != character.GetNode<Camera2D>("PlayerCamera"))
			{
				Vector2 defaultier = Vector2.Zero;
				CameraManager.SetActiveCamera(character.GetNode<Camera2D>("PlayerCamera"));
				CameraManager.SetCameraPosition(defaultier);
			}
		}
		else
		{
			if (CameraManager.GetActiveCamera() != GetNode<Camera2D>("WorldCamera"))
			{
				Vector2 defaultiest = CameraManager.GetActiveCamera().GlobalPosition;
				CameraManager.SetActiveCamera(GetNode<Camera2D>("WorldCamera"));
				CameraManager.SetGlobalCameraPosition(defaultiest);
			}
		}

		foreach (var room in changedRooms)
		{
			UpdateRoom(room, room.IsPowered || room != currentRoom);
		}
		changedRooms.Clear();

		base._PhysicsProcess(delta);
	}

	public override void _Ready()
	{
		RoomManager = new(this);
		CameraManager = new(this);
		ResourceManager = new(this);

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


		var roomTemplate = GetNode("RoomTemplates");
		var lootNodePrototypes = GetNode("LootNodePrototypes");
		var lootNodeContainer = GetNode("LootNodes");
		var startRoom = GetNode<TileMap>("StartRoom");
		var startCamera = GetNode<Camera2D>("WorldCamera");
		CameraManager.SetActiveCamera(startCamera);
		worldMap = GetNode<TileMap>("World");
		overlayMap = GetNode<TileMap>("WorldOverlay");
		WorldGenerator = new WorldGenerator(this, worldMap, startRoom, roomTemplate, lootNodePrototypes, lootNodeContainer);
		WorldGenerator.InactiveTileCoordinates = inactiveTileCoordinates;
		WorldGenerator.Generate();
		changedRooms.AddRange(RoomManager.AllRooms);
	}
}
