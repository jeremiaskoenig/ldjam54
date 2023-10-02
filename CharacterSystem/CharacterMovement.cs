using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CharacterMovement : Node2D
{
	private Queue<Vector2> currentPath = new();
	private Vector2? currentTarget;
	private Character parent;

	private float speed = 0;

	private long CalculateId(Vector2 pos)
	{
		var a = pos.X;
		var b = pos.Y;
		return (long)((a + b) * (a + b + 1) / 2 + b);
	}

	public override void _Ready()
	{
		parent = GetParent<Character>();
		speed = (float)GetMeta("moveSpeed");
		base._Ready();
	}

	public override void _Input(InputEvent e)
	{
		if (parent.Selection.IsSelected)
		{
			if (e is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == MouseButton.Right)
			{
				if (e.IsReleased())
				{
					TriggerMovement(GetGlobalMousePosition());
				}
			}
		}

		base._Input(e);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (currentTarget == null && currentPath.Count > 0)
		{
			currentTarget = currentPath.Dequeue(); 
		}

		if (currentTarget != null)
		{
			var target = currentTarget.Value;
			var direction = new Vector2(target.X - parent.GlobalPosition.X, target.Y - parent.GlobalPosition.Y).Normalized();
			parent.Translate(direction * speed);
			if (parent.GlobalPosition.DistanceTo(target) <= (speed * 1.5))
			{
				currentTarget = null;
				parent.GlobalPosition = target;
			}
		}

		base._PhysicsProcess(delta);
	}

	public void TriggerMovement(Vector2 target)
	{
		var main = GetTree().Root.GetNode <Main>("Main");
		var worldGen = main.WorldGenerator;
		int tileSize = main.GetConfig<int>("tileSize");
		var start = worldGen.WalkableTiles.OrderBy(node => parent.GlobalPosition.DistanceTo(scaled(node))).First();
		var end = worldGen.WalkableTiles.OrderBy(node => target.DistanceTo(scaled(node))).First();

		var path = main.WorldGenerator.AStar.GetPointPath(CalculateId(start), CalculateId(end));

		currentPath.Clear();
		foreach (var node in path.Select(vec => scaled(vec)))
		{
			currentPath.Enqueue(node);
		}


		Vector2 scaled(Vector2 vec) => new((vec.X * tileSize) + (tileSize * 0.5f), (vec.Y * tileSize) + (tileSize * 0.5f));
	}
}
