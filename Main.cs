using Godot;
using System;

public partial class Main : Node2D
{
	//public Node2D[] BrokenRooms { get; set; }
	//public Node StartRoom { get; set; }
	//
	private RoomGenerator roomGenerator;

	public override void _Ready()
	{
		var roomTemplate = GetTree().Root.GetNode<Node2D>("Main/RoomTemplate");
		var roomContainer = GetTree().Root.GetNode<Node2D>("Main/Rooms");
		roomGenerator = new RoomGenerator(roomContainer, roomTemplate);
		roomGenerator.Generate();
	}
}
