using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class WorldGenerator
{
	private readonly Main main;
	private readonly TileMap worldMap;
	private readonly TileMap startRoom;
	private readonly Node lootNodeContainer;
	private readonly Node[] lootNodeTemplates;
	private readonly List<TileMap> rooms = new();
	public Vector2I InactiveTileCoordinates { get; set; }

	public AStar2D AStar { get; } = new AStar2D();
	public List<Vector2I> WalkableTiles { get; } = new List<Vector2I>();

	public WorldGenerator(Main main, TileMap worldMap, TileMap startRoom, Node roomTemplates, Node lootNodeTemplates, Node lootNodeContainer)
	{
		this.main = main;
		this.worldMap = worldMap;
		this.startRoom = startRoom;
		this.lootNodeContainer = lootNodeContainer;
		this.lootNodeTemplates = lootNodeTemplates.GetChildren().ToArray();
		foreach (var node in roomTemplates.GetChildren())
		{
			if (node is TileMap room)
			{
				rooms.Add(room);
			}
		}
	}

	public void Generate()
	{
		var roomWidth = main.GetConfig<int>("roomWidth");
		var roomHeight = main.GetConfig<int>("roomHeight");
		var worldSize = main.GetConfig<int>("worldSize");
		var tileSize = main.GetConfig<int>("tileSize");

		for (int y = 0; y < worldSize; y++)
		{
			for (int x = 0; x < worldSize; x++)
			{
				var offsetX = x * roomWidth;
				var offsetY = y * roomHeight;

				TileMap roomMap;
				bool isPowered = false;

				if (x == 5 && y == 5)
				{
					roomMap = startRoom;
					isPowered = true;
				}
				else
				{
					roomMap = rooms[GD.RandRange(0, rooms.Count - 1)];
				}

				var room = main.RoomManager.RegisterRoom(new Vector2I(x, y));
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

		startRoom.QueueFree();
		rooms.ForEach(room => room.QueueFree());

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
