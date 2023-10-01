using Godot;
using System.Collections.Generic;

public class CameraManager
{
    private readonly Main main;
    private Camera2D ActiveCamera;

    public CameraManager(Main main)
    {
        this.main = main;
    }
    public Camera2D GetActiveCamera()
    {
        return ActiveCamera;
    }
    public void SetActiveCamera(Camera2D camera)
    {
        camera.MakeCurrent();
        ActiveCamera = camera;
    }
    public void SetCameraPosition(Vector2 targetCoordinates)
    {
        ActiveCamera.Position = targetCoordinates;
    }

    public void SetGlobalCameraPosition(Vector2 targetCoordinates)
    {
        ActiveCamera.GlobalPosition = targetCoordinates;
    }
    public void SetCameraLimits()
    {
        ActiveCamera.LimitTop = -104;
        ActiveCamera.LimitLeft = -104;
        ActiveCamera.LimitRight = main.GetConfig<int>("roomWidth") * main.GetConfig<int>("worldSize") * 16 + 104;
        ActiveCamera.LimitBottom = main.GetConfig<int>("roomHeight") * main.GetConfig<int>("worldSize") * 16 + 104;
    }
}