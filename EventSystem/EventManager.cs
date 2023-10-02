using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class EventManager
{
    private readonly Main main;
    private CameraManager cameraManager;
    private bool escapeShipTrigger;
    private EscapeEvent escapeShip;
    private Vector2 escapeShipTarget;
    private bool lockCamera;

    public bool moveShip;

    public EventManager(Main main)
    {
        this.main = main;
        this.escapeShip = main.GetNode<Node>("Worldobjects").GetNode<EscapeEvent>("EscapeShip");
        this.cameraManager = main.CameraManager;
        this.moveShip = false;
        this.lockCamera = false;
    }

    public void SetEscapeShipTrigger()
    {
        escapeShipTrigger = true;
        main.GetNode<UserInterface>("UserInterface").IsHUDVisible = false;
        EscapeShipTriggered();
    }

    public void EscapeShipTriggered()
    {
        escapeShipTarget = new Vector2(2100, 650);
        escapeShip.Visible = true;
        escapeShip.Position = new Vector2(2100, 1800);
        lockCamera = true;
        var shipCamera = escapeShip.GetNode<Camera2D>("ShipCamera");
        cameraManager.SetActiveCamera(shipCamera);
        cameraManager.SetCameraLimits();

        moveShip = true;
    }

    public bool GetMoveShip()
    {
        if (!escapeShipTrigger)
            return false;

        if (escapeShipTarget == escapeShip.GlobalPosition)
        {
            moveShip = false;
            
            escapeShip.wait(1000);
            CharacterEndSceneMove();
            moveShip = true;
            escapeShipTarget = new Vector2(2100,-1000);
        }
        
        return moveShip;
    }

    private void CharacterEndSceneMove()
    {
        foreach (var character in main.Characters)
        {
            character.Movement.TriggerMovement(new Vector2(2100, 650));
        }   
    }

    public void initiateEndscreen()
    {
        var canvaslayer = main.GetNode<CanvasLayer>("Transitionscreen");
        var animationplayer = main.GetNode<CanvasLayer>("Transitionscreen").GetNode<AnimationPlayer>("AnimationPlayer");
        canvaslayer.Visible = true;
        animationplayer.Play("fade_to_black");
        canvaslayer.GetNode<Label>("Label").Visible = true;
    }

    public Node2D GetEscapeShip()
    {
        return escapeShip;
    }
    public bool GetCameraLock()
    {
        return lockCamera;
    }
    public Node2D GetCharacter()
    {
        return main.GetNode<Node>("Characters").GetNode<Node2D>("Character1");
    }
}
