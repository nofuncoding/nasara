using Godot;
using System;

public partial class NavBar : Control
{

	ButtonGroup buttonGroup;

	string currentNav = "editor"; // TODO: Bad hard coding

	[Export]
	Button editorButton;

	[Export]
	Button projectButton;

	[Export]
	Label versionLabel;

	[Signal]
	public delegate void NavChangedEventHandler(StringName nav);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// TODO: use ButtonGroup to manage navs more easily
		/*
		buttonGroup = GD.Load<ButtonGroup>("res://component/nav_button_group.tres");
		buttonGroup.Pressed += NavChanged;
		*/

		editorButton.Pressed += EditorButtonPressed;
		projectButton.Pressed += ProjectButtonPressed;

		versionLabel.Text = (string)ProjectSettings.GetSetting("application/config/version");
	}
/*
    private void NavChanged(BaseButton button)
    {
        switch (button)
		{
		}
    }
*/

	void EditorButtonPressed()
	{
		if (currentNav != "editor")
		{
			EmitSignal(SignalName.NavChanged, "EditorView");
			currentNav = "editor";
		}
	}

	void ProjectButtonPressed()
	{
		if (currentNav != "project")
		{
			EmitSignal(SignalName.NavChanged, "ProjectView");
			currentNav = "project";
		}
	}
}
