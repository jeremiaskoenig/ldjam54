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
			}

			string spawnedCharacter = (string)roomMap.SafeGetMeta("spawnedCharacter", default);
			Action enterTrigger = null;
			if (!String.IsNullOrEmpty(spawnedCharacter))
			{
				enterTrigger = () =>
				{
					var spawnPos = new Vector2((offsetX + (roomWidth * 0.5f)) * tileSize, (offsetY + (roomHeight * 0.5f)) * tileSize);
					main.SpawnCharacter(spawnedCharacter, spawnPos);
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
						spawnedBuildable = buildable;
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
