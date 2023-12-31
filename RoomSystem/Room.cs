using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Room
{
	public Vector2I Coordinates { get; }

    private List<(Vector2I, int)> overlayTiles = new();
    public List<(Vector2I cell, int layer)> OverlayTiles => overlayTiles;
    public List<Vector2I> WorldMapTiles { get; } = new();
    public List<Vector2I> BuildableWorldMapTiles { get; } = new();
    public List<Vector2I> LootSpawnWorldMapTiles { get; } = new();

	public bool IsPowered { get; set; }

    private readonly Action enterTrigger;
    private bool isTriggered = false;

	public Room(Vector2I coordinates, Action enterTrigger)
	{
		Coordinates = coordinates;
        this.enterTrigger = enterTrigger;
    }

    public void FinalizeRoomInitialization()
    {
        overlayTiles = overlayTiles.Distinct().ToList();
    }

	public void Trigger()
    {
        if (isTriggered)
            return;

        isTriggered = true;
        enterTrigger?.Invoke();
    }
}
