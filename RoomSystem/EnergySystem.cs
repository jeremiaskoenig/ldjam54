using System.Linq;

public class EnergySystem
{
    private readonly Main main;

    public EnergySystem(Main main)
    {
        this.main = main;
    }

    public int UsedPower { get; private set; }
    public int TotalPower { get; private set; }

    public void UpdatePower()
    {
        UsedPower = main.RoomManager.AllRooms.Count(room => room.IsPowered);
        TotalPower = main.GetNode("Buildables").GetChildren().Where(node => (string)node.GetMeta("buildableType") == "generator").Count();
    }
}