using Godot;
using System.Collections.Generic;

public partial class WorldGenerator
{
	private readonly Main main;
	private readonly TileMap worldMap;
	private readonly TileMap startRoom;
	private readonly List<TileMap> rooms = new();
	public Vector2I InactiveTileCoordinates { get; set; }

	public AStar2D AStar { get; } = new AStar2D();
	public List<Vector2I> WalkableTiles { get; } = new List<Vector2I>();

	public WorldGenerator(Main main, TileMap worldMap, TileMap startRoom, Node roomTemplates)
	{
		this.main = main;
		this.worldMap = worldMap;
		this.startRoom = startRoom;
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

		for (int y = 0; y < worldSize; y++)
		{
			for (int x = 0; x < worldSize; x++)
			{
				var offsetX = x * roomWidth;
				var offsetY = y * roomHeight;

				TileMap room;
				bool isPowered = false;

				if (x == 5 && y == 5)
				{
					room = startRoom;
					isPowered = true;
				}
				else
				{
					room = rooms[GD.RandRange(0, rooms.Count - 1)];
				}

				for (int layer = 0; layer < room.GetLayersCount(); layer++)
				{
					var layerCells = room.GetUsedCells(layer);

					foreach (var cell in layerCells)
					{
						var cellPos = new Vector2I(offsetX + cell.X, offsetY + cell.Y);

						var sourceId = room.GetCellSourceId(layer, cell);
						var atlasCoords = room.GetCellAtlasCoords(layer, cell);

						worldMap.SetCell(layer, cellPos, sourceId, atlasCoords);
					}
				}

				var roomObj = main.RoomManager.RegisterRoom(new Vector2I(x, y));
				roomObj.IsPowered = isPowered;
				var cells = room.GetUsedCells(0);

				foreach (var cell in cells)
				{
					var cellPos = new Vector2I(offsetX + cell.X, offsetY + cell.Y);
					roomObj.WorldMapTiles.Add(cellPos);
					if (!isPowered)
					{
						var sourceId = room.GetCellSourceId(0, cell);
						worldMap.SetCell(2, cellPos, sourceId, InactiveTileCoordinates);
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

		foreach (var cell in floorCells)
		{
			foreach (var neighbour in neighbours)
			{
				var neighbourCell = cell + neighbour;
				if (floorCells.Contains(neighbourCell))
				{
					AStar.ConnectPoints(id(cell), id(neighbourCell), false);
				}
			}
		}


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
