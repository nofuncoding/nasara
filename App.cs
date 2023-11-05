using Godot;
using System;

public partial class App : Control
{

	[Export]
	NavBar navBar;

	[Export]
	ViewSwitch viewSwitch;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		navBar.NavChanged += (string nav) => viewSwitch.SwitchView(nav);
	}
}
