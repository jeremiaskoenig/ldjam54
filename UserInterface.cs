using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UserInterface : CanvasLayer
{
    private Main main;
    private Control buildPanel;
    private Control resourcePanel;
    private Dictionary<string, BuildingManager.Buildable> buildables = new()
    {
        { "PowerGenerator", null },
        { "OxygenGenerator", null },
        { "Repair", null }
    };

    public bool IsMouseOver { get; private set; }

    public override void _Ready()
    {
        main = GetTree().Root.GetNode<Main>("Main");
        buildPanel = GetNode<Control>("BuildPanel");
        resourcePanel = GetNode<Control>("Resources");

        SetupUIMouseOver(resourcePanel);

        SetupBuildPanel();

        Visible = true;
        base._Ready();
    }

    private void SetupBuildPanel()
    {
        SetupUIMouseOver(buildPanel);

        AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Texts/PowerGenerator"));
        AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Texts/OxygenGenerator"));
        AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Texts/Repair"));
        AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Icons/PowerGenerator"));
        AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Icons/OxygenGenerator"));
        AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Icons/Repair"));

        SetupBuildingButtons();
    }

    private void SetupBuildingButtons()
    {
        setupPressed("PowerGenerator");
        setupPressed("OxygenGenerator");
        setupPressed("Repair");
        void setupPressed(string key)
        {
            GetNode<BaseButton>($"BuildPanel/Buttons/{key}").Pressed += () => main.BuildingManager.Build(buildables[key], main.SelectedCharacter.GlobalPosition);
        }
    }

    private void SetupUIMouseOver(Control element)
    {
        element.MouseEntered += () => { IsMouseOver = true; };
        element.MouseExited += () => { IsMouseOver = false; };

        foreach (Control child in element.GetChildren())
        {
            SetupUIMouseOver(child);
        }
    }

    private void AttachDescriptionHandlers(Control control)
    {
        var descriptionLabel = GetNode<Label>("BuildPanel/Texts/Description");
        control.MouseEntered += () =>
        {
            if (!buildPanel.Visible)
                return;

            var buildable = buildables[control.Name];
            string description = "";
            if (buildable != null)
            {
                description += String.Join(", ", buildable.Cost.Select(cost => $"{cost.amount}\u00A0{cost.type.GetName()}")) + "\n";
                description += buildable.Description;
            }
            descriptionLabel.Text = description;
        };

        control.MouseExited += () =>
        {
            descriptionLabel.Text = "";
        };
    }

    public override void _Process(double delta)
    {
        SetResourceLabel("BuildingMaterials", ResourceType.BuildingMaterials);
        SetResourceLabel("DuctTape", ResourceType.DuctTape);
        SetResourceLabel("Chemicals", ResourceType.Chemicals);
        SetResourceLabel("Tools", ResourceType.Tools);
        SetResourceLabel("Circuitry", ResourceType.Circuitry);
        SetResourceLabel("Power", $"{main.EnergySystem.UsedPower}/{main.EnergySystem.TotalPower}");

        UpdateBuildPanel();

        base._Process(delta);
    }

    private void UpdateBuildPanel()
    {
        var selectedCharacter = main.SelectedCharacter;

        if (selectedCharacter != null)
        {
            var room = main.RoomManager.GetRoom(selectedCharacter.GlobalPosition);

            Vector2I playerPos = main.RoomManager.WorldToRoom(selectedCharacter.GlobalPosition);
            var visible = room.BuildableWorldMapTiles.Contains(playerPos);
            buildPanel.Visible = visible;
            if (visible)
            {
                var buildables = main.BuildingManager.AvailableBuildables(selectedCharacter.GlobalPosition);
                foreach (var buildable in buildables)
                {
                    Label label;
                    if (!buildable.IsRepair)
                    {
                        label = GetNode<Label>($"BuildPanel/Texts/{buildable.Key}");
                        label.Text = buildable.Name;
                        this.buildables[buildable.Key] = buildable;
                    }
                    else
                    {
                        label = GetNode<Label>("BuildPanel/Texts/Repair");
                        label.Text = $"Repair {buildable.Name}";
                        this.buildables["Repair"] = buildable;
                    }
                }

                bool isRepair = buildables.Any(buildable => buildable.IsRepair);

                GetNode<Control>("BuildPanel/Texts/PowerGenerator").Visible = !isRepair;
                GetNode<Control>("BuildPanel/Icons/PowerGenerator").Visible = !isRepair;
                GetNode<Control>("BuildPanel/Buttons/PowerGenerator").Visible = !isRepair;
                GetNode<Control>("BuildPanel/Texts/OxygenGenerator").Visible = !isRepair;
                GetNode<Control>("BuildPanel/Icons/OxygenGenerator").Visible = !isRepair;
                GetNode<Control>("BuildPanel/Buttons/OxygenGenerator").Visible = !isRepair;
                GetNode<Control>("BuildPanel/Texts/Repair").Visible = isRepair;
                GetNode<Control>("BuildPanel/Icons/Repair").Visible = isRepair;
                GetNode<Control>("BuildPanel/Buttons/Repair").Visible = isRepair;

            }

        }
        else
        {
            buildPanel.Visible = false;
        }
    }

    private void SetResourceLabel(string name, string text)
    {
        GetNode<Label>($"Resources/Texts/{name}").Text = text;
    }

    private void SetResourceLabel(string name, ResourceType resource)
    {
        SetResourceLabel(name, main.ResourceManager.Resources[resource].ToString());
    }
}
