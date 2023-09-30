using Godot;
using System;
using System.Collections.Generic;

public partial class WorldGenerator : GodotObject
{
	private readonly Main main;
	private readonly TileMap worldMap;
	private readonly TileMap roomTemplate;

	public AStar2D AStar { get; } = new AStar2D();
	public List<Vector2I> WalkableTiles { get; } = new List<Vector2I>();

	public WorldGenerator(Main main, TileMap worldMap, TileMap roomTemplate)
	{
		this.main = main;
		this.worldMap = worldMap;
		this.roomTemplate = roomTemplate;

		GD.Print(worldMap);
		GD.Print(roomTemplate);
	}

	public void Generate()
	{
		var roomWidth = main.GetConfig<int>("roomWidth");
		var roomHeight = main.GetConfig<int>("roomHeight");

		for (int y = 0; y < 10; y++)
		{
			for (int x = 0; x < 10; x++)
			{
				var offsetX = x * roomWidth;
				var offsetY = y * roomHeight;

				for (int layer = 0; layer < roomTemplate.GetLayersCount(); layer++)
				{
					var cells = roomTemplate.GetUsedCells(layer);

					foreach (var cell in cells)
					{
						var cellPos = new Vector2I(offsetX + cell.X, offsetY + cell.Y);

						var sourceId = roomTemplate.GetCellSourceId(layer, cell);
						var atlasCoords = roomTemplate.GetCellAtlasCoords(layer, cell);

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
