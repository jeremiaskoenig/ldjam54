using Godot;

public partial class Character : Node2D
{
	private Main main;

	public CameraMovement Camera => GetNode<CameraMovement>("PlayerCamera");
	public CharacterSelection Selection => GetNode<CharacterSelection>("Selection");
	public CharacterMovement Movement => GetNode<CharacterMovement>("Movement");

	public float Oxygen { get; private set; } = 1000f;

	public override void _Ready()
	{
		main = GetTree().Root.GetNode<Main>("Main");
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		var currentRoom = main.RoomManager.GetRoom(GlobalPosition);

		var oxygenLoss = main.GetConfig<float>("oxygenLoss");

		Oxygen += currentRoom.IsPowered ? oxygenLoss : -oxygenLoss;
		Oxygen = Mathf.Clamp(Oxygen, 0, 1000);

		base._PhysicsProcess(delta);
	}
}
