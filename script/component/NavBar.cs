using Godot;
using System;
using System.Collections.Generic;

public partial class NavBar : Control
{
	ButtonGroup buttonGroup;

	int currentNav = 0;
	int availableViewsCount = 0;

	Godot.Collections.Dictionary<int, BaseButton> buttonDictionary = new();

	[Export]
	Label versionLabel;

	[Export]
	BaseButton infoButton;

	[Signal]
	public delegate void NavigatedEventHandler(int nav);

	[Signal]
	public delegate void ViewRegisteredEventHandler(int index, PackedScene packedScene);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		buttonGroup = GD.Load<ButtonGroup>("res://component/nav_button_group.tres");
		buttonGroup.Pressed += NavChanged;
		infoButton.Pressed += () => {
			Window AppInfo = GD.Load<PackedScene>("res://component/popup/app_info_popup.tscn").Instantiate<Window>();
			AddChild(AppInfo);
			AppInfo.PopupCentered();
		};

		if (OS.IsDebugBuild())
		{
			versionLabel.Visible = true;
			versionLabel.Text = "v" + (string)ProjectSettings.GetSetting("application/config/version");
		} else {
			versionLabel.Visible = false;
		}

	}

	public int RegisterView(PackedScene packedScene, string displayName, int viewIndex = -1)
	{
		int index = viewIndex;
		if (index <= -1)
		{
			int i = 0;
			for(;;)
			{
				if (buttonDictionary.ContainsKey(i)) // Find out available index
					i++;
				else
					break;
			}
			
			index = i;
		}

        Button viewButton = new()
        {
            Text = displayName,
			ToggleMode = true,
			ButtonGroup = buttonGroup
        };
        GetNode("VBoxContainer").AddChild(viewButton);

		if (index == 0) // Default View
			viewButton.ButtonPressed = true;
		
		buttonDictionary.Add(index, viewButton);
		EmitSignal(SignalName.ViewRegistered, index, packedScene);


		availableViewsCount++;
		return index;
	}

    private void NavChanged(BaseButton button)
    {

        foreach (KeyValuePair<int, BaseButton> viewButton in buttonDictionary)
		{

			if (viewButton.Value == button)
			{
				if (viewButton.Key == currentNav)
					return;

				EmitSignal(SignalName.Navigated, viewButton.Key);
				currentNav = viewButton.Key;
			}
		}
    }
}
