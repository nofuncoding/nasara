using Godot;
using System;

public partial class App : Control
{
	// GodotManager godotManager;

	[Export]
	NavBar navBar;

	[Export]
	ViewSwitch viewSwitch;

	/*[Export]
	Godot.Collections.Dictionary<string, string> viewsPath;*/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitViews();
	}

	void InitViews()
	{
		navBar.Navigated += (int nav) => viewSwitch.SwitchView(nav);
		navBar.ViewRegistered += (int index, PackedScene packedScene) => viewSwitch.packedView.Add(index, packedScene);

		/*
		foreach (string path in viewsPath)
		{
			Node node = GD.Load<PackedScene>(path);
			navBar.RegisterView
		}*/

		navBar.RegisterView(GD.Load<PackedScene>("res://view/editor_view.tscn"), "Editor", 0);
		navBar.RegisterView(GD.Load<PackedScene>("res://view/project_view.tscn"), "Project");

		viewSwitch.Init();
	}
}
