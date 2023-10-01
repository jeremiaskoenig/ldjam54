using Godot;
using System.Collections.Generic;
using System.Linq;

public class RoomManager
{
    private readonly Main main;
    private readonly List<Room> rooms = new();
    public IEnumerable<Room> AllRooms => rooms;

    public RoomManager(Main main)
    {
        this.main = main;
    }

    public Room RegisterRoom(Vector2I coordinates)
    {
        var room = new Room(coordinates);
        rooms.Add(room);    
        return room;
    }

    public Room GetRoom(Vector2 point)
    {
        var roomWidth = main.GetConfig<int>("roomWidth");
        var roomHeight = main.GetConfig<int>("roomHeight");
        var tileSize = main.GetConfig<int>("tileSize");

        var roomX = (int)(point.X / (roomWidth * tileSize));
        var roomY = (int)(point.Y / (roomHeight * tileSize));

        return rooms.Where(room => room.Coordinates == new Vector2I(roomX, roomY)).FirstOrDefault();
    }

    internal Vector2I WorldToRoom(Vector2 point)
    {
        var roomWidth = main.GetConfig<int>("roomWidth");
        var roomHeight = main.GetConfig<int>("roomHeight");
        var tileSize = main.GetConfig<int>("tileSize");

        var posX = (int)((point.X % (roomWidth * tileSize)) / tileSize);
        var posY = (int)((point.Y % (roomHeight * tileSize)) / tileSize);

        return new Vector2I(posX, posY);
    }
}
