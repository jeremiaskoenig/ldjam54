using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	private TileMap worldMap;

	public WorldGenerator WorldGenerator  { get; private set; }
	public RoomManager RoomManager { get; private set; }

	private readonly Dictionary<string, object> configuration = new();

	private Vector2I inactiveTileCoordinates;
	private Vector2I hiddenTileCoordinates;

	private Room previousRoom;
	private readonly List<Room> changedRooms = new();

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

	private void UpdateRoomOverlayLayer(Room room, bool isHidden)
	{
		GD.Print($"room at {room.Coordinates}; powered: {room.IsPowered}; hidden: {isHidden}");
		const int VISIBLE = 0;
		const int INVISIBLE = -1;

		foreach (var cell in room.WorldMapTiles)
		{
			var overlay = isHidden ? hiddenTileCoordinates : inactiveTileCoordinates;

			int isOverlayVisible = room.IsPowered ? INVISIBLE : VISIBLE;

			var sourceId = worldMap.GetCellSourceId(0, cell);
			worldMap.SetCell(2, cell, sourceId, overlay, isOverlayVisible);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		var character = GetNode<Node2D>("Character");
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

		foreach (var room in changedRooms)
		{
			UpdateRoomOverlayLayer(room, room.IsPowered || room != currentRoom);
		}
		changedRooms.Clear();

		base._PhysicsProcess(delta);
	}

	public override void _Ready()
	{
		RoomManager = new(this);

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
		var startRoom = GetNode<TileMap>("StartRoom");
		worldMap = GetNode<TileMap>("World");
		WorldGenerator = new WorldGenerator(this, worldMap, startRoom, roomTemplate);
		WorldGenerator.InactiveTileCoordinates = inactiveTileCoordinates;
		WorldGenerator.Generate();
		changedRooms.AddRange(RoomManager.AllRooms);
	}
}
