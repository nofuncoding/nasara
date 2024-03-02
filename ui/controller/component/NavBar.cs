using Godot;
using System;
using System.Collections.Generic;

namespace Nasara.UI.Component;

public partial class NavBar : PanelContainer
{
	ButtonGroup buttonGroup;

	Godot.Collections.Dictionary<int, BaseButton> buttonDictionary = [];

	[Export]
	ViewSwitch viewSwitch;

	[Export]
	Label versionLabel;

	[Export]
	MenuButton menuButton;

	public override void _Ready()
	{
		buttonGroup = GD.Load<ButtonGroup>("res://ui/component/nav_button_group.tres");
		buttonGroup.Pressed += ChangeNav;
		
		PopupMenu menu = menuButton.GetPopup();
		menu.Theme = GD.Load<Theme>("res://res/style/normal_control_theme.tres");
		menu.RemoveThemeFontOverride("font"); // Do not use theme override of the menu button
		menu.AddItem(Tr("About")+"...", 0);
		menu.IdPressed += MenuIdPressed;
		menu.Transparent = true;
		menu.TransparentBg = true;

		if (OS.IsDebugBuild()) // Display version
		{
			versionLabel.Visible = true;
			versionLabel.Text = "v" + (string)ProjectSettings.GetSetting("application/config/version");
		} else {
			versionLabel.Visible = false;
		}

	}

	void MenuIdPressed(long id)
	{
		switch (id)
		{
			case 0:
				// open about dialog
				Window AppInfo = GD.Load<PackedScene>("res://ui/component/popup/app_info_popup.tscn").Instantiate<Window>();
				AddChild(AppInfo);
				AppInfo.PopupCentered();
				break;
		}
	}

	public int RegisterView(PackedScene packedScene, string displayName)
	{
		int index = viewSwitch.AddView(packedScene);
		
		Button viewButton = new()
		{
			Text = displayName,
			ToggleMode = true,
			ButtonGroup = buttonGroup
		};
		GetNode("VBoxContainer/ButtonList").AddChild(viewButton);

		if (index == 0) // Default View
			viewButton.ButtonPressed = true;
		
		buttonDictionary.Add(index, viewButton);

		return index;
	}

	private void ChangeNav(BaseButton button)
	{
		foreach (KeyValuePair<int, BaseButton> viewButton in buttonDictionary)
		{
			if (viewButton.Value == button)
			{
				if (viewButton.Key == viewSwitch.CurrentPage)
					return;

				viewSwitch.CurrentPage = viewButton.Key;
			}
		}
	}
}
