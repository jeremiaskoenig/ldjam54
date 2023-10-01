using Godot;
using System;

public partial class UserInterface : CanvasLayer
{
    private Main main;

    public override void _Ready()
    {
        main = GetTree().Root.GetNode<Main>("Main");
        
        Visible = true;
        base._Ready();
    }

    public override void _Process(double delta)
    {
        SetResourceLabel("BuildingMaterials", ResourceType.BuildingMaterials);
        SetResourceLabel("DuctTape", ResourceType.DuctTape);
        SetResourceLabel("Chemicals", ResourceType.Chemicals);
        SetResourceLabel("Tools", ResourceType.Tools);
        SetResourceLabel("Circuitry", ResourceType.Circuitry);

        base._Process(delta);
    }

    private void SetResourceLabel(string name, ResourceType resource)
    {
        GetNode<Label>($"Resources/Texts/{name}").Text = main.ResourceManager.Resources[resource].ToString();
    }
}
