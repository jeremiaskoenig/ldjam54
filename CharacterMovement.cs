using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CharacterMovement : Node2D
{
    private Queue<Vector2> currentPath = new();
    private Vector2? currentTarget;
    private Node2D parent;

    private float speed = 0;

    private long CalculateId(Vector2 pos)
    {
        var a = pos.X;
        var b = pos.Y;
        return (long)((a + b) * (a + b + 1) / 2 + b);
    }

    public override void _Ready()
    {
        parent = GetParent<Node2D>();
        speed = (float)GetMeta("moveSpeed");
        base._Ready();
    }

    public override void _Input(InputEvent e)
    {
        if (IsCharacterSelected())
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
            if (parent.GlobalPosition.DistanceTo(target) <= 2)
            {
                currentTarget = null;
                parent.GlobalPosition = target;
            }
        }
        base._PhysicsProcess(delta);
    }

    private void TriggerMovement(Vector2 target)
    {
        var main = GetTree().Root.GetNode <Main>("Main");
        var worldGen = main.WorldGenerator;
        int tileSize = main.GetConfig<int>("tileSize");
        var start = worldGen.WalkableTiles.OrderBy(node => parent.GlobalPosition.DistanceTo(new Vector2(scaled(node.X), scaled(node.Y)))).First();
        var end = worldGen.WalkableTiles.OrderBy(node => target.DistanceTo(new Vector2(scaled(node.X), scaled(node.Y)))).First();

        var path = main.WorldGenerator.AStar.GetPointPath(CalculateId(start), CalculateId(end));

        currentPath.Clear();
        foreach (var node in path.Select(vec => new Vector2(scaled(vec.X), scaled(vec.Y))))
        {
            currentPath.Enqueue(node);
        }

        float scaled(float tileCoord)
        {
            int tileSize = GetTree().Root.GetNode<Main>("Main").GetConfig<int>("tileSize");
            return (tileSize * tileCoord) + (tileSize * 0.5f);
        }
    }

    private bool IsCharacterSelected()
    {
        var selection = GetParent().GetNode<CharacterSelection>("Selection");
        return selection.IsSelected;
    }
}
