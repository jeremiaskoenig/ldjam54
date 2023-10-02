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
        EscapeShipTriggered();    }

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
        if (escapeShipTarget == escapeShip.GlobalPosition)
        {
            moveShip = false;
            
            escapeShip.wait(7500);
            //disableUI
            CharacterEndSceneMove();
            moveShip = true;
            escapeShipTarget = new Vector2(2100,-1000);
            initiateEndscreen();
        }
        
        return moveShip;
    }

    private void CharacterEndSceneMove()
    {
        foreach (var character in main.Characters)
        {
            character.Movement.EventMovement(new Vector2(2100, 650));
        }   
    }

    private void initiateEndscreen()
    {
        //TODO:BITTE BITTE BAU ENDSCREEN JA SAG WALLAh
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
