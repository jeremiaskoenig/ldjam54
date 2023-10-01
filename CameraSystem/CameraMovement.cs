using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudumDare54.CameraSystem
{
	public partial class CameraMovement : Camera2D
	{
		private CameraManager cameraManager;
		private Vector2 mouse_start_pos = Vector2.Zero;
		private Vector2 screen_start_position = Vector2.Zero;

		public override void _Input(InputEvent e)
		{
			if(cameraManager == null)
			{
				cameraManager = GetTree().Root.GetNode<Main>("Main").CameraManager;
			}
			var camera = cameraManager.GetActiveCamera();
			var zoom_min = new Vector2((float)1, (float)1);
			var zoom_max = new Vector2((float)3, (float)3);
			var zoom_speed = new Vector2((float).2, (float).2);



			if (e is InputEventMouseButton mouseButtonEvent)
			{
				if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
				{
					if (e.IsPressed())
					{
						if (camera.Zoom > zoom_min)
						{
							camera.Zoom -= zoom_speed;
						}
					}

				}
				else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp)
				{
					if (e.IsPressed())
					{
						if (camera.Zoom < zoom_max)
						{
							camera.Zoom += zoom_speed;
						}
					}
				}

				if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
				{
					if (e.IsPressed())
					{
						camera.DragHorizontalEnabled = true;
						camera.DragVerticalEnabled = true;
						mouse_start_pos = mouseButtonEvent.Position;	
						screen_start_position = camera.Position;
					}
					else if (e.IsReleased())
					{
						camera.DragHorizontalEnabled = false;
						camera.DragVerticalEnabled = false; ;
					}
				}
			}
			else if (e is InputEventMouseMotion mouseMotionEvent && DragHorizontalEnabled)
            {
				camera.Position = screen_start_position + camera.Zoom * (mouse_start_pos - mouseMotionEvent.Position);
			}
			
			base._Input(e);
		}
	}
}
