using Godot;
using Nasara.Core;
using Nasara.UI;

namespace Nasara;

public class App
{
	public const string CachePath = "user://cache";

	public static NotificationSystem NotificationSystem => _notificationSystemInstance;
	

	private static App _instance;
	private static AppLayout _layoutInstance;
	private static NotificationSystem _notificationSystemInstance;

	private static Core.Management.Editor.GodotManager _godotManager;

	private App()
	{
		CreateDirs();
		NetworkClient.Initialize();
	}

	public static void Initialize(AppLayout layout)
	{
		if (_instance is not null) return;
		
		GD.PrintRich($"[b]Nasara v{GetVersion()}[/b] by NoFun\n" +
		             "https://github.com/nofuncoding/nasara\n");
		
		Logger.Log("Initializing");
        
		_instance = new App();
		_layoutInstance = layout;
		_notificationSystemInstance = new NotificationSystem();
		_godotManager = new Core.Management.Editor.GodotManager();
	}

	public static App Get() => _instance;
	public static AppLayout GetLayout() => _layoutInstance;
	public static Core.Management.Editor.GodotManager GetGodotManager() => _godotManager;
	
	/// <summary>
	/// Get the current version of app
	/// </summary>
	/// <returns>Version string</returns>
	public static string GetVersion() => (string)ProjectSettings.GetSetting("application/config/version");

	private static void CreateDirs()
	{
		// The directories need to create when starting
		string[] dirs = [CachePath];

		foreach (var d in dirs)
		{
			// If not exists then create it.
			if (!DirAccess.DirExistsAbsolute(d))
			{
				DirAccess.MakeDirRecursiveAbsolute(d);
			}
		}
	}
}