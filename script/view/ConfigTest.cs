using Godot;
using System;

public partial class ConfigTest : Control
{
	
	AppConfig appConfig;

	public override void _Ready()
	{
		appConfig = new AppConfig();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<Label>("Label").Text = appConfig.TestVar.ToString();
	}

	public void _OnButtonToggled(bool toggle_on)
	{
		appConfig.TestVar = toggle_on;
	}
}
