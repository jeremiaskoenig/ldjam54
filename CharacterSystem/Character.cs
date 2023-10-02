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

		Oxygen += currentRoom.IsPowered ? (20 * oxygenLoss) : -oxygenLoss;
		Oxygen = Mathf.Clamp(Oxygen, -60, 1000);

		if (Oxygen == 0)
		{
			var canvaslayer = main.GetNode<CanvasLayer>("Transitionscreen");
			var animationplayer = main.GetNode<CanvasLayer>("Transitionscreen").GetNode<AnimationPlayer>("AnimationPlayer");
			canvaslayer.Visible = true;
			animationplayer.Play("fade_to_black");
			canvaslayer.GetNode<Label>("LabelFailure").Visible = true;
		}
		else if (Oxygen == -100)
		{
			GetTree().ReloadCurrentScene();
		}

		base._PhysicsProcess(delta);
	}
}
