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

	GodotManager.Manager godotManager;
	GodotManager.VersionList versionList;

	public Godot.Collections.Array<DownloadableVersion> stableVersions;
	public Godot.Collections.Array<DownloadableVersion> unstableVersions;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		godotManager = GetNode<GodotManager.Manager>("/root/GodotManager"); // Autoload
		versionList = new();
		AddChild(versionList);

		versionList.GetList += (Godot.Collections.Dictionary<string, Godot.Collections.Array<DownloadableVersion>> list) => {
			stableVersions = list["stable"];
			unstableVersions = list["unstable"];
		};

		GD.Print("(app) Initializing");
		Init();
	}

	void Init()
	{
		string lang = new AppConfig().Language;
		if (lang != "")
			TranslationServer.SetLocale(lang);

		mainPage.Visible = false;
		loadingPage.Visible = true;

		loadingBar.MaxValue = 1;
		loadingBar.MaxValue += 2;
//		loadingBar.MaxValue += views.Count() * 2 + 1; // InitViews

		CreateDirs(); // important, do not delete it

		InitViews();
		if (versionList.GetGodotList(godotManager) != Error.Ok)
			GD.PushError("(app) Failed to load Godot List!");
		
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
	public NotifySystem GetNotifySystem()
	{
		return notifySystem;
	}
}
