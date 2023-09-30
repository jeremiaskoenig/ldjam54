using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CharacterMovement : Node2D
{
    private Queue<Vector2> currentPath = new();
    private Vector2? currentTarget;
    private Node2D parent;
    private AStar2D astar = new AStar2D();
    private List<(int id, Vector2 position)> nodeList = new();
    private List<Room> calculatedRooms = new();

    private float speed = 0;

    public override void _Process(double delta)
    {
        var nodes = GetTree().Root.GetNode<Node2D>("Main/Rooms").GetChildren().OfType<Room>().ToList();

        if (nodes.Count != calculatedRooms.Count)
        {
            int id = nodeList.Any() ? nodeList.Max(node => node.id) : 0;
            foreach (var room in nodes)
            {
                if (calculatedRooms.Contains(room))
                    continue;

                calculatedRooms.Add(room);

                for (int y = 0; y < 7; y++)
                {
                    for (int x = 0; x < 7; x++)
                    {
                        var currentId = id++;
                        var currentPosition = new Vector2(room.GlobalPosition.X + (x * 18), room.GlobalPosition.Y + (y * 18));
                        var currentNode = (id: currentId, position: currentPosition);

                        var weight = 0;
                        if (x == 0 && y != 3)
                            weight = Int32.MaxValue;
                        if (y == 0 && x != 3)
                            weight = Int32.MaxValue;

                        if (weight != 0)
                        {
                            var collider = GetNode<Node2D>("TileCollider").Duplicate() as Node2D;
                            GetTree().Root.AddChild(collider);
                            collider.GlobalPosition = currentPosition;
                            collider.Visible = true;
                        }

                        nodeList.Add(currentNode);
                        astar.AddPoint(currentNode.id, currentNode.position, weight);
                    }
                }
            }

            foreach (var node in nodeList)
            {
                foreach (var otherNode in nodeList.Where(isNeighbour))
                {
                    astar.ConnectPoints(node.id, otherNode.id);
                }

                bool isNeighbour((int id, Vector2 position) otherNode)
                {
                    var validPositions = new[]
                    {
                    new Vector2(node.position.X - 18, node.position.Y),
                    new Vector2(node.position.X + 18, node.position.Y),
                    new Vector2(node.position.X, node.position.Y - 18),
                    new Vector2(node.position.X, node.position.Y + 18),
                };

                    return validPositions.Contains(otherNode.position);
                }
            }
        }

        base._Process(delta);
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
                    TriggerMovement(mouseButtonEvent.GlobalPosition);
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
        var start = nodeList.OrderBy(node => parent.GlobalPosition.DistanceTo(node.position)).First();
        var end = nodeList.OrderBy(node => target.DistanceTo(node.position)).First();

        var path = astar.GetIdPath(start.id, end.id);
        currentPath.Clear();
        foreach (var node in path.Select(id => nodeList.First(node => node.id == id)))
        {
            currentPath.Enqueue(node.position);
        }

        //currentPath.Enqueue(target);
    }

    private bool IsCharacterSelected()
    {
        var selection = GetParent().GetNode<CharacterSelection>("Selection");
        return selection.IsSelected;
    }
}
