using Godot;
using System;

public partial class Door : Node2D
{
    private bool isOpened = false;
    public bool IsOpened
    {
        get => isOpened;
        set
        {
            isOpened = value;
            if (isOpened)
            {
                GetNode<Sprite2D>("Sprite").Visible = false;
                OpenAction?.Invoke();
            }
        }
    }

    public Action OpenAction { get; set; }
}
