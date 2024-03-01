using Godot;
using System;
using Nasara.UI;
using Nasara.UI.Component;
using Editor = Nasara.Core.Management.Editor;
using Project = Nasara.Core.Management.Project;

namespace Nasara;

public partial class App : PanelContainer
{
//	[Export]
//  [{"name": string, "path": string}, ...]
//	Godot.Collections.Array<Godot.Collections.Dictionary<string, string>> views;

	// HttpClient is intended to be instantiated once per application, rather than per-use.
	public static readonly System.Net.Http.HttpClient sysHttpClient = new() { Timeout = TimeSpan.FromSeconds(60) };

	private static App instance;

	public const string CACHE_PATH = "user://cache";

	[Export(PropertyHint.NodePathToEditedNode)]
	NodePath notifySystemPath;

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

	static Editor.Manager _editorManager;
	static Project.Manager _projectManager;
	static NotifySystem notifySystem;
	Editor.VersionList versionList;

	public Godot.Collections.Array<Editor.DownloadableVersion> stableVersions;
	public Godot.Collections.Array<Editor.DownloadableVersion> unstableVersions;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("(app) Initializing");
		Init();
	}

	void Init()
	{
		instance = this;
		notifySystem = GetNode<NotifySystem>(notifySystemPath);
		Core.Network.Github.Requester.Init();

		AppConfig config = new();
		string lang = config.Language;
		if (lang != "")
			TranslationServer.SetLocale(lang);
		
		_editorManager = new();
		_projectManager = new();
		AddChild(_editorManager);
		AddChild(_projectManager);

		versionList = new();
		AddChild(versionList);

		versionList.GetList += (Godot.Collections.Dictionary<string, Godot.Collections.Array<Editor.DownloadableVersion>> list) => {
			stableVersions = list["stable"];
			unstableVersions = list["unstable"];
		};
		
		InitStyles();

		mainPage.Visible = false;
		loadingPage.Visible = true;

		loadingBar.MaxValue = 1;
		loadingBar.MaxValue += 2;
//		loadingBar.MaxValue += views.Count() * 2 + 1; // InitViews

		CreateDirs(); // important, do not delete it

		InitViews();
		if (versionList.GetGodotList(_editorManager) != Error.Ok)
			GD.PushError("(app) Failed to load Godot List!");
		
		loadingBar.Value++;

		mainPage.Visible = true;
		loadingPage.Visible = false; 
	}

	void InitStyles()
	{
		AppConfig config = new();

		GetTree().Root.TransparentBg = config.TransparentBackground;

		var splitContainer = GetNode<VSplitContainer>("VSplitContainer");
		
		if (config.UseCustomTitlebar)
		{
			GetTree().Root.Borderless = true;
			var titlebar = GD.Load<PackedScene>("res://ui/component/custom_window_bar.tscn").Instantiate();
			splitContainer.AddChild(titlebar);
			splitContainer.MoveChild(titlebar, 0);
		}

		if (!config.TransparentBackground || !config.UseCustomTitlebar)
		{
			var appPanel = GD.Load<StyleBoxFlat>("res://res/style/app_panel.tres");
			appPanel.SetCornerRadiusAll(0);
		}
	}

	void InitViews()
	{
		navBar.Navigated += (int nav) => viewSwitch.SwitchView(nav);
		navBar.ViewRegistered += viewSwitch.packedView.Add;

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

		navBar.RegisterView(GD.Load<PackedScene>("res://ui/view/editor_view.tscn"), Tr("Editor"), 0);
		navBar.RegisterView(GD.Load<PackedScene>("res://ui/view/project_view.tscn"), Tr("Project"));
		navBar.RegisterView(GD.Load<PackedScene>("res://ui/view/news_view.tscn"), Tr("News"));
		navBar.RegisterView(GD.Load<PackedScene>("res://ui/view/setting_view.tscn"), Tr("Setting"));

		viewSwitch.Init();
		loadingBar.Value++;
	}

	static void CreateDirs()
	{
		string[] dirs = [CACHE_PATH];

		foreach (string d in dirs)
		{
			if (!DirAccess.DirExistsAbsolute(d))
			{
				DirAccess.MakeDirRecursiveAbsolute(d);
			}
		}
	}

	public static NotifySystem GetNotifySystem() => notifySystem;

	public static Editor.Manager GetEditorManager() => _editorManager;
	
	public static Project.Manager GetProjectManager() => _projectManager;

	public static App GetInstance() => instance;
}
