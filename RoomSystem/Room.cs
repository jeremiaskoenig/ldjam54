using Godot;
using System.Collections.Generic;

public class Room
{
	public Vector2I Coordinates { get; }

	public List<Vector2I> WorldMapTiles { get; } = new();

	public bool IsPowered { get; set; }

	public Room(Vector2I coordinates)
	{
		Coordinates = coordinates;
	}
}
