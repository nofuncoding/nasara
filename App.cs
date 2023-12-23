using Godot;
using System;
using System.Linq;

public partial class App : Control
{
//	[Export]
//  [{"name": string, "path": string}, ...]
//	Godot.Collections.Array<Godot.Collections.Dictionary<string, string>> views;

	[Export]
	NotifySystem notifySystem;

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

	public static readonly string GODOT_LIST_CACHE_PATH = "user://cache/remote_godot.json";

	string GodotCurrentNodeId = null;
	string GodotUnstableCurrentNodeId = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		godotManager = GetNode<GodotManager>("/root/GodotManager");
		godotRequester = godotManager.GetRequester();

		godotRequester.VersionsRequested += (Godot.Collections.Array<DownloadableVersion> downloadableVersions, int channel) =>
		{
			switch (channel)
			{
				case (int)GodotVersion.VersionChannel.Stable:
					stableVersions = downloadableVersions; break;
				case (int)GodotVersion.VersionChannel.Unstable:
					unstableVersions = downloadableVersions; break;
			}
		};

		GD.Print("Initializing App");
		Init();
	}

	void Init()
	{
		/*
		notifySystem.Notify(description: "AS");
		notifySystem.Notify(NotifyBallon.NotifyType.Warn, description: "AS1");
		notifySystem.Notify(NotifyBallon.NotifyType.Error, description: "AS2");
		*/
		mainPage.Visible = false;
		loadingPage.Visible = true;

		loadingBar.MaxValue = 1;
		loadingBar.MaxValue += 2;
//		loadingBar.MaxValue += views.Count() * 2 + 1; // InitViews

		CreateDirs(); // important, do not delete it

		InitViews();
		if (GetGodotList() != Error.Ok)
			GD.PushError("Failed to load Godot List!");
		
		loadingBar.Value++;

		loadingPage.Visible = false; 
		mainPage.Visible = true;
	}

	void InitViews()
	{
		navBar.Navigated += (int nav) => viewSwitch.SwitchView(nav);
		navBar.ViewRegistered += (int index, PackedScene packedScene) => viewSwitch.packedView.Add(index, packedScene);

//		Godot.Collections.Dictionary<string, PackedScene> loadedView = new();

/*		BUG: Can't use dictionary to init views.
		foreach (Godot.Collections.Dictionary v in views)
		{
			PackedScene packedScene = GD.Load<PackedScene>((string)v["path"]);

			loadedView.Add((string)v["name"], packedScene);
			loadingBar.Value++;
		}

		foreach (var (name, packed) in loadedView)
			navBar.RegisterView(packed, name); Stopped running here
*/

		navBar.RegisterView(GD.Load<PackedScene>("res://view/editor_view.tscn"), Tr("Editor"), 0);
		navBar.RegisterView(GD.Load<PackedScene>("res://view/project_view.tscn"), Tr("Project"));
		navBar.RegisterView(GD.Load<PackedScene>("res://view/setting_view.tscn"), Tr("Setting"));

		viewSwitch.Init();
		loadingBar.Value++;
	}

	void CreateDirs()
	{
		string[] dirs = new string[] { "user://cache" };

		foreach (string d in dirs)
		{
			if (!DirAccess.DirExistsAbsolute(d))
			{
				DirAccess.MakeDirRecursiveAbsolute(d);
			}
		}
	}

	// FIXME: Buggy Cache Updating
	Error GetGodotList()
	{
		if (!FileAccess.FileExists(GODOT_LIST_CACHE_PATH)) {
			godotRequester.RequestEditorList();
			godotRequester.RequestEditorList(GodotVersion.VersionChannel.Unstable);

			return Error.Ok;
		}

		godotRequester.RequestLatestNodeId();
		godotRequester.RequestLatestNodeId(GodotVersion.VersionChannel.Unstable);

		godotRequester.NodeIdRequested += (string nodeId, int channel) => { // Updating Cache
			switch (channel)
			{
				case (int)GodotVersion.VersionChannel.Stable:
					if (GodotCurrentNodeId != nodeId && nodeId is not null)
					{
						GD.Print("Updating Cache for Godot Stable");
						godotRequester.RequestEditorList();
					}
					break;
				case (int)GodotVersion.VersionChannel.Unstable:
					if (GodotUnstableCurrentNodeId != nodeId && nodeId is not null)
					{
						GD.Print("Updating Cache for Godot Unstable");
						godotRequester.RequestEditorList(GodotVersion.VersionChannel.Unstable);
					}
					break;
			}
		};

		return ProcessGodotListCache();
	}

	Error ProcessGodotListCache()
	{
		using var file = FileAccess.Open(GODOT_LIST_CACHE_PATH, FileAccess.ModeFlags.ReadWrite);
		if (file is null)
			return FileAccess.GetOpenError();
		
		// Get storaged json data
		Json fileJson = new();
		if (fileJson.Parse(file.GetAsText()) != Error.Ok)
			return Error.ParseError;

		GD.Print($"Reading from cache {GODOT_LIST_CACHE_PATH}");

		Godot.Collections.Dictionary version_dict = (Godot.Collections.Dictionary)fileJson.Data;

		if (version_dict.ContainsKey("stable"))
		{
			Godot.Collections.Dictionary latest = 
			(Godot.Collections.Dictionary)((Godot.Collections.Array)version_dict["stable"])[0];

			GodotCurrentNodeId = (string)latest["node_id"];

			string data = Json.Stringify(version_dict["stable"]);
			stableVersions = godotRequester.ProcessRawData(data, GodotVersion.VersionChannel.Stable);
		} else {
			// GodotRequester will auto save cache
			godotRequester.RequestEditorList();
		}

		if (version_dict.ContainsKey("unstable"))
		{
			Godot.Collections.Dictionary latest = 
			(Godot.Collections.Dictionary)((Godot.Collections.Array)version_dict["unstable"])[0];

			GodotUnstableCurrentNodeId = (string)latest["node_id"];

			string data = Json.Stringify(version_dict["unstable"]);
			unstableVersions = godotRequester.ProcessRawData(data, GodotVersion.VersionChannel.Unstable);
		} else {
			// GodotRequester will auto save cache
			godotRequester.RequestEditorList(GodotVersion.VersionChannel.Unstable);
		}
		
		return Error.Ok;
	}

	public NotifySystem GetNotifySystem()
	{
		return notifySystem;
	}
}
