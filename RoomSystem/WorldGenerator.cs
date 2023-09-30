using Godot;
using System;

public partial class RoomGenerator : GodotObject
{
	private readonly Node2D mainNode;
	private readonly Node2D roomTemplate;

	public RoomGenerator(Node2D mainNode, Node2D roomTemplate)
	{
		this.mainNode = mainNode;
		this.roomTemplate = roomTemplate;
	}

	public void Generate()
	{
		for (int y = 0; y < 10; y++)
		{
			for (int x = 0; x < 10; x++)
			{
				var newNode = roomTemplate.Duplicate() as Node2D;
				newNode.Translate(new Vector2(126 * x, 126 * y));
				newNode.Visible = true;

				mainNode.AddChild(newNode);
			}
		}

		// prep:
		// create a list of "broken" rooms



		// get main node
		// iterate from min to max width
		// iterate from min to max height
		// select and place random room from list
		// if index is 0|0 create start room instead

	}
}
