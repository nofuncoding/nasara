using Godot;
using System;

public partial class SettingView : Control
{
	[ExportGroup("Network")]
	[Export]
	CheckButton enableTLS;
	[Export]
	CheckButton githubProxy;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AppConfig config = new();
		enableTLS.ButtonPressed = config.EnableTLS;
		githubProxy.ButtonPressed = config.UsingGithubProxy;

		enableTLS.Toggled += (bool s) => config.EnableTLS = s;
		githubProxy.Toggled += (bool s) => config.UsingGithubProxy = s;
	}
}
