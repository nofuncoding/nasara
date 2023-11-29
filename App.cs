using Godot;
using System;

public partial class App : Control
{
	// GodotManager godotManager;

	[ExportGroup("Pages")]
	[Export]
	Control mainPage;
	[Export]
	Control loadingPage;

	[ExportSubgroup("Main Page")]
	[Export]
	NavBar navBar;

	[Export]
	ViewSwitch viewSwitch;

	[ExportSubgroup("Loading Page")]
	[Export]
	ProgressBar loadingBar;

	GodotManager godotManager;
	GodotRequester godotRequester;

	public Godot.Collections.Array<DownloadableVersion> stableVersions;
	public Godot.Collections.Array<DownloadableVersion> unstableVersions;

	/*[Export]
	Godot.Collections.Dictionary<string, string> viewsPath;*/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		godotManager = GetNode<GodotManager>("/root/GodotManager");
		godotRequester = godotManager.GetRequester();

		godotRequester.RequestCompleted += (Godot.Collections.Array<DownloadableVersion> downloadableVersions, int channel) =>
		{
			switch (channel)
			{
				case (int)GodotVersion.VersionChannel.Stable:
					stableVersions = downloadableVersions; break;
				case (int)GodotVersion.VersionChannel.Unstable:
					unstableVersions = downloadableVersions; break;
			}

			loadingBar.Value++;
		};

		Init();
	}

	void Init()
	{
		// TODO: Refactor to get flexible
		mainPage.Visible = false;
		loadingPage.Visible = true;

		loadingBar.MaxValue = 6;

		InitViews(); // Value added inside
		godotRequester.RequestEditorList(); // Value added inside
		godotRequester.RequestEditorList(GodotVersion.VersionChannel.Unstable);

		mainPage.Visible = true;
		loadingPage.Visible = false;
	}

	void InitViews()
	{
		navBar.Navigated += (int nav) => viewSwitch.SwitchView(nav);
		navBar.ViewRegistered += (int index, PackedScene packedScene) => viewSwitch.packedView.Add(index, packedScene);
		loadingBar.Value++;

		/*
		foreach (string path in viewsPath)
		{
			Node node = GD.Load<PackedScene>(path);
			navBar.RegisterView
		}*/

		navBar.RegisterView(GD.Load<PackedScene>("res://view/editor_view.tscn"), "Editor", 0);
		loadingBar.Value++;
		navBar.RegisterView(GD.Load<PackedScene>("res://view/project_view.tscn"), "Project");
		loadingBar.Value++;

		viewSwitch.Init();
		loadingBar.Value++;
	}

	void RequestGodotList(GodotVersion.VersionChannel channel)
	{

	}
}
