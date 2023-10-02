using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WorldGenerator
{
	private readonly Main main;
	private readonly TileMap worldMap;
	private readonly Node lootNodeContainer;
	private readonly Node[] lootNodeTemplates;
	private readonly List<TileMap> rooms = new();
	private readonly List<TileMap> storyRooms = new();
	public Vector2I InactiveTileCoordinates { get; set; }

	public AStar2D AStar { get; } = new AStar2D();
	public List<Vector2I> WalkableTiles { get; } = new List<Vector2I>();

	public WorldGenerator(Main main, TileMap worldMap, Node roomTemplates, Node storyRoomTemplates, Node lootNodeTemplates, Node lootNodeContainer)
	{
		this.main = main;
		this.worldMap = worldMap;
		this.lootNodeContainer = lootNodeContainer;
		this.lootNodeTemplates = lootNodeTemplates.GetChildren().ToArray();
		foreach (var node in roomTemplates.GetChildren())
		{
			if (node is TileMap room)
			{
				rooms.Add(room);
			}
		}
		foreach (var node in storyRoomTemplates.GetChildren())
		{
			if (node is TileMap room)
			{
				storyRooms.Add(room);
			}
		}
	}

	public void Generate()
	{
		var roomWidth = main.GetConfig<int>("roomWidth");
		var roomHeight = main.GetConfig<int>("roomHeight");
		var worldSize = main.GetConfig<int>("worldSize");
		var tileSize = main.GetConfig<int>("tileSize");

		var worldAtlas = main.GetNode<TileMap>("WorldAtlas");

		var worldCells = worldAtlas.GetUsedCells(0);

        foreach (var worldCell in worldCells)
        {
			int x = worldCell.X;
			int y = worldCell.Y;

			var offsetX = x * roomWidth;
			var offsetY = y * roomHeight;

			TileMap roomMap;
			bool isPowered = false;

			var allTemplates = rooms.Concat(storyRooms);

			var mapRoomType = worldAtlas.GetCellAtlasCoords(0, worldCell);

			var pool = allTemplates.Where(template => (Vector2I)template.GetMeta("roomType") == mapRoomType).ToArray();
			if (pool.Length == 0)
            {
				GD.PushWarning($"No room found for mapRoomType {mapRoomType}");
            }

			roomMap = pool[GD.RandRange(0, pool.Length - 1)];
			isPowered = (bool)roomMap.SafeGetMeta("isPowered", false);

			Vector2I spawnPlayer = (Vector2I)roomMap.SafeGetMeta("spawnPlayer", -Vector2I.One);
			if (spawnPlayer != -Vector2I.One)
            {
				var playerSpawn = new Vector2((offsetX + spawnPlayer.X) * tileSize + (tileSize * 0.5f), (offsetY + spawnPlayer.Y) * tileSize + (tileSize * 0.5f));
				main.Characters.First().GlobalPosition = playerSpawn;
				main.CameraManager.GetActiveCamera().GlobalPosition = playerSpawn;
				//Center cam
			}

			string spawnedCharacter = (string)roomMap.SafeGetMeta("spawnedCharacter", "");
			string trigger = (string)roomMap.SafeGetMeta("trigger", "");
			Action enterTrigger = null;
			if (!String.IsNullOrEmpty(trigger))
            {
				switch (trigger)
                {
					case "ending":
						enterTrigger = () =>
						{
							main.EventManager.SetEscapeShipTrigger();
						};
						break;
                }
            }
			else if (!String.IsNullOrEmpty(spawnedCharacter))
			{
				enterTrigger = () =>
				{
					var characterSpawnPos = new Vector2((offsetX + (roomWidth * 0.5f)) * tileSize, (offsetY + (roomHeight * 0.5f)) * tileSize);
					main.SpawnCharacter(spawnedCharacter, characterSpawnPos);

					var generatorSpawnPos = new Vector2((offsetX + 2) * tileSize + (0.5f * tileSize), (offsetY + 6) * tileSize + (0.5f * tileSize));
					main.BuildingManager.Spawn(main.BuildingManager.Buildables["PowerGenerator"], generatorSpawnPos);
					main.RoomManager.GetRoom(characterSpawnPos).IsPowered = true;
				};
			}

            foreach (var meta in roomMap.GetMetaList())
            {
				var metaName = (string)meta;
				if (!metaName.StartsWith("spawn_"))
					continue;
				var metaValue = (Vector2I)roomMap.GetMeta(meta);

				BuildingManager.Buildable spawnedBuildable = null;

                foreach (var buildable in main.BuildingManager.Buildables)
				{
					if (metaName.StartsWith($"spawn_{buildable.Key}"))
                    {
						spawnedBuildable = buildable.Value;
						break;
                    }
				}

				if (spawnedBuildable != null)
                {
					GD.Print($"Spawning {spawnedBuildable.Key}@{metaValue} in room with offset ({offsetX}, {offsetY})");
					var spawnPosition = new Vector2((offsetX + metaValue.X) * tileSize + (tileSize * 0.5f), (offsetY + metaValue.Y) * tileSize + (tileSize * 0.5f));
					main.BuildingManager.Spawn(spawnedBuildable, spawnPosition);
                }
                else
                {
					GD.PushWarning($"no buildable found for spawn {metaName}");
                }
            }

			var room = main.RoomManager.RegisterRoom(new Vector2I(x, y), enterTrigger);
			room.IsPowered = isPowered;

			for (int layer = 0; layer < roomMap.GetLayersCount(); layer++)
			{
				var layerCells = roomMap.GetUsedCells(layer);

				foreach (var cell in layerCells)
				{
					var cellPos = new Vector2I(offsetX + cell.X, offsetY + cell.Y);

					room.OverlayTiles.Add((cellPos, layer));

					if (layer == 0)
						room.WorldMapTiles.Add(cellPos);

					var sourceId = roomMap.GetCellSourceId(layer, cell);
					var atlasCoords = roomMap.GetCellAtlasCoords(layer, cell);

					var tileData = roomMap.GetCellTileData(layer, cell);
					var canSpawnLoot = (bool)tileData.GetCustomData("canSpawnLoot");
					var canBeBuiltOn = (bool)tileData.GetCustomData("canBeBuiltOn");

					if (canSpawnLoot)
					{
						if (GD.Randf() <= 0.2f)
						{
							var lootNode = lootNodeTemplates[GD.RandRange(0, lootNodeTemplates.Length - 1)].Duplicate() as Node2D;
							lootNode.Visible = true;
							lootNode.GlobalPosition = (cellPos * tileSize) + new Vector2(tileSize * 0.5f, tileSize * 0.5f);
							lootNodeContainer.AddChild(lootNode);
						}
						room.LootSpawnWorldMapTiles.Add(cellPos);
					}

					if (canBeBuiltOn)
						room.BuildableWorldMapTiles.Add(cellPos);

					worldMap.SetCell(layer, cellPos, sourceId, atlasCoords);
				}
			}

			room.FinalizeRoomInitialization();
		}

		var floorCells = worldMap.GetUsedCells(0);
		var wallCells = worldMap.GetUsedCells(1);

		foreach (var cell in floorCells)
		{
			if (wallCells.Contains(cell))
				continue;
			AStar.AddPoint(id(cell), cell, 0);
			WalkableTiles.Add(cell);
		}

		foreach (var cell in WalkableTiles)
		{
			foreach (var neighbour in neighbours)
			{
				var neighbourCell = cell + neighbour;
				if (WalkableTiles.Contains(neighbourCell))
				{
					AStar.ConnectPoints(id(cell), id(neighbourCell), false);
				}
			}
		}

		Vector2I endDoorTilePosition = new(103, 40);
		Vector2I fuelDoorTilePosition = new(72, 33);
		Vector2I[] doorTilePositions = new[]
		{
			new Vector2I(8, 49),
			new Vector2I(8, 40),
			new Vector2I(34, 31),
			new Vector2I(39, 22),
			new Vector2I(34, 4)
		};

		AStar.DisconnectPoints(id(endDoorTilePosition), id(endDoorTilePosition + Vector2I.Right), true);
		AStar.DisconnectPoints(id(fuelDoorTilePosition), id(fuelDoorTilePosition + Vector2I.Right), true);
		foreach (var pos in doorTilePositions)
		{
			AStar.DisconnectPoints(id(pos), id(pos + Vector2I.Right), true);
		}

		Vector2 endDoorPosition = new(1656, 648);
		Vector2 fuelDoorPosition = new(1160, 536);
		Vector2[] doorPositions = new[]
		{
			new Vector2(136, 792),
			new Vector2(136, 648),
			new Vector2(552, 504),
			new Vector2(632, 360),
			new Vector2(552, 72)
		};

		BuildingManager.Buildable doorBuildable = main.BuildingManager.Door;
		var endDoor = main.BuildingManager.Spawn(doorBuildable, endDoorPosition) as Door;
		var fuelDoor = main.BuildingManager.Spawn(doorBuildable, fuelDoorPosition) as Door;
		int doorIndex = 0;
		foreach (var pos in doorPositions)
        {
			var doorTilePos = doorTilePositions[doorIndex];
			var door = main.BuildingManager.Spawn(doorBuildable, pos) as Door;
			door.OpenAction = () =>
			{
				AStar.ConnectPoints(id(doorTilePos), id(doorTilePos + Vector2I.Right), true);
			};
			
			door.Name = $"ComputerDoor_{doorIndex++}";

		}

		endDoor.Name = "EscapeDoor";
		endDoor.OpenAction = () =>
		{
			AStar.ConnectPoints(id(endDoorTilePosition), id(endDoorTilePosition + Vector2I.Right), true);
		};

		fuelDoor.Name = "FuelDoor";
		fuelDoor.OpenAction = () =>
		{
			AStar.ConnectPoints(id(fuelDoorTilePosition), id(fuelDoorTilePosition + Vector2I.Right), true);
		};

		rooms.ForEach(room => room.QueueFree());
		storyRooms.ForEach(room => room.QueueFree());

		long id(Vector2 pos) => CalculateId(pos);
	}

	private static readonly Vector2I[] neighbours = new[]
	{
		new	Vector2I(1,0),
		new	Vector2I(-1,0),
		new	Vector2I(0,1),
		new	Vector2I(0,-1),
	};

	private long CalculateId(Vector2 pos)
	{
		var a = pos.X;
		var b = pos.Y;
		return (long)((a + b) * (a + b + 1) / 2 + b);
	}
}
