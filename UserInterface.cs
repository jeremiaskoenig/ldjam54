using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UserInterface : CanvasLayer
{
	private Main main;
	private Control buildPanel;
	private Control resourcePanel;
    private Control oxygenPanel;
    private Control powerRoomPanel;
	
	private Dictionary<string, BuildingManager.Buildable> buildables = new()
	{
		{ "PowerGenerator", null },
		{ "OxygenGenerator", null },
		{ "Repair", null }
	};
	private bool isOxygenPanelOpened = true;
	private int oxygenPanelOpenedOffset = -40;
	private int oxygenPanelClosedOffset = 167;
	private int oxygenPanelSpeed = 600;

    public bool IsMouseOver { get; private set; }

	public override void _Ready()
    {
        main = GetTree().Root.GetNode<Main>("Main");

        SetupResourcePanel();
        SetupBuildPanel();
        SetupOxygenPanel();
		SetupPowerRoomPanel();

        Visible = true;
        base._Ready();
    }

    private void SetupPowerRoomPanel()
    {
		powerRoomPanel = GetNode<Control>("PowerRoomPanel");
		SetupUIMouseOver(powerRoomPanel);

		var toggleButton = powerRoomPanel.GetNode<BaseButton>("Button");
		toggleButton.Pressed += () =>
		{
			var room = main.RoomManager.GetRoom(main.SelectedCharacter.GlobalPosition);
			room.IsPowered = !room.IsPowered;
			main.EnergySystem.UpdatePower();
			main.RefreshRoom(room);
		};
	}

	private void SetupResourcePanel()
    {
        resourcePanel = GetNode<Control>("Resources");
        SetupUIMouseOver(resourcePanel);
    }

    private void SetupOxygenPanel()
    {
		oxygenPanel = GetNode<Control>("OxygenPanel");
		SetupUIMouseOver(oxygenPanel);

		var toggleButton = GetNode<BaseButton>("ToggleOxygenPanel");
		toggleButton.Pressed += () =>
		{
			isOxygenPanelOpened = !isOxygenPanelOpened;
			GetNode<TextureRect>("ToggleOxygenPanel/Open").Visible = !isOxygenPanelOpened;
			GetNode<TextureRect>("ToggleOxygenPanel/Close").Visible = isOxygenPanelOpened;
		};
	}

    private void SetupBuildPanel()
	{
		buildPanel = GetNode<Control>("BuildPanel");
		SetupUIMouseOver(buildPanel);

		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Texts/PowerGenerator"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Texts/OxygenGenerator"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Texts/Repair"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Icons/PowerGenerator"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Icons/OxygenGenerator"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Icons/Repair"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Buttons/PowerGenerator"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Buttons/OxygenGenerator"));
		AttachDescriptionHandlers(GetNode<Control>("BuildPanel/Buttons/Repair"));

		SetupBuildingButtons();
	}

	private void SetupBuildingButtons()
	{
		setupPressed("PowerGenerator");
		setupPressed("OxygenGenerator");
		setupPressed("Repair");
		void setupPressed(string key)
		{
			GetNode<BaseButton>($"BuildPanel/Buttons/{key}").Pressed += () =>
			{
				GD.Print($"pressed: {key}");
				main.BuildingManager.Build(buildables[key], main.SelectedCharacter.GlobalPosition);
				main.EnergySystem.UpdatePower();
			};
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
        UpdateResourcePanel();
        UpdateBuildPanel();
        UpdateOxygenPanel((float)delta);
		UpdatePowerRoomPanel();

        base._Process(delta);
    }

    private void UpdatePowerRoomPanel()
	{
		var selectedCharacter = main.SelectedCharacter;

		if (selectedCharacter != null)
		{
			var tileSize = main.GetConfig<int>("tileSize");
			var room = main.RoomManager.GetRoom(selectedCharacter.GlobalPosition);

			Vector2I playerPos = new((int)(selectedCharacter.GlobalPosition.X / tileSize), (int)(selectedCharacter.GlobalPosition.Y / tileSize));

			var visible = room.BuildableWorldMapTiles.Contains(playerPos) && main.BuildingManager.IsType(playerPos, "oxygen_gen");
			powerRoomPanel.Visible = visible;
			if (visible)
			{
				var button = powerRoomPanel.GetNode<BaseButton>("Button");
				button.Disabled = !room.IsPowered && main.EnergySystem.UsedPower >= main.EnergySystem.TotalPower;

				var label = powerRoomPanel.GetNode<Label>("ButtonText");
				label.Text = $"Power {(room.IsPowered ? "Down" : "Up")}";
			}

		}
		else
		{
			powerRoomPanel.Visible = false;
		}
	}

    private void UpdateResourcePanel()
    {
        SetResourceLabel("BuildingMaterials", ResourceType.BuildingMaterials);
        SetResourceLabel("DuctTape", ResourceType.DuctTape);
        SetResourceLabel("Chemicals", ResourceType.Chemicals);
        SetResourceLabel("Tools", ResourceType.Tools);
        SetResourceLabel("Circuitry", ResourceType.Circuitry);
        SetResourceLabel("Power", $"{main.EnergySystem.UsedPower}/{main.EnergySystem.TotalPower}");
    }

    private void UpdateOxygenPanel(float delta)
    {
		var step = delta * oxygenPanelSpeed;
		step *= isOxygenPanelOpened ? -1 : 1;
		var viewRect = oxygenPanel.GetViewportRect();
		var pos = oxygenPanel.GlobalPosition - viewRect.Size;
		var newX = Mathf.Clamp(pos.X + step, oxygenPanelOpenedOffset, oxygenPanelClosedOffset);
		oxygenPanel.GlobalPosition = new(newX + viewRect.Size.X, oxygenPanel.GlobalPosition.Y);

		foreach (Godot.Range oxygenBar in oxygenPanel.GetNode("Bars").GetChildren())
		{
			var character = main.GetCharacter(oxygenBar.Name);

			if (character != null)
            {
				oxygenBar.Value = character.Oxygen * 0.1;

				oxygenBar.Visible = true;
				oxygenPanel.GetNode<Control>($"Texts/{oxygenBar.Name}").Visible = true;
            }
            else
            {
				oxygenBar.Visible = false;
				oxygenPanel.GetNode<Control>($"Texts/{oxygenBar.Name}").Visible = false;
			}
		}
	}

	private void UpdateBuildPanel()
	{
		var selectedCharacter = main.SelectedCharacter;

		if (selectedCharacter != null)
		{
			var tileSize = main.GetConfig<int>("tileSize");
			var room = main.RoomManager.GetRoom(selectedCharacter.GlobalPosition);

			Vector2I playerPos = new((int)(selectedCharacter.GlobalPosition.X / tileSize), (int)(selectedCharacter.GlobalPosition.Y / tileSize));

			var visible = room.BuildableWorldMapTiles.Contains(playerPos) && main.BuildingManager.IsClear(playerPos);
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
