using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class EscapeEvent : Node2D
{
	private Main main;
	private EventManager eventManager;
	private Node2D escapeShip;
	private int waitTimer;
	private Node2D drive1;
	private Sprite2D drive2;
	private Node2D character;
	public override void _Ready()
	{
		main = GetTree().Root.GetNode<Main>("Main");
		base._Ready();
	}
	public override void _PhysicsProcess(double delta)
	{
		if(eventManager == null)
		{
			eventManager = main.EventManager;
		}
		if(escapeShip == null)
		{
			escapeShip = eventManager.GetEscapeShip();
		}
		if(drive1 == null ||drive2 == null)
		{
			drive1 = escapeShip.GetNode<Sprite2D>("Ship").GetNode<Sprite2D>("Drive1");
			drive2 = escapeShip.GetNode<Sprite2D>("Ship").GetNode<Sprite2D>("Drive2");
		}

		if (eventManager.GetMoveShip())
		{
			if(character == null)
            {
				character = eventManager.GetCharacter();
            }

			if (waitTimer > 0)
			{
				if(waitTimer < 200)
                {
					character.Visible = false;
                }

				waitTimer -= 5; 
				drive1.Scale = new Vector2(0, 1);
				drive2.Scale = new Vector2(0, 1);
			}
			else
			{
				escapeShip.Position -= new Vector2(0, 5);
				var rando =(float)0.5 + GD.Randf();
				drive1.Scale = new Vector2((float)1.2, rando);
				drive2.Scale = new Vector2((float)1.2, rando);
			}
		}
		
		base._PhysicsProcess(delta);
	}

	public void wait(int Timer)
	{
		waitTimer = Timer;
	}
}
