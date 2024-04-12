using Godot;
using System;

namespace Nasara;

public class App
{
	// HttpClient is intended to be instantiated once per application, rather than per-use.
	public static readonly System.Net.Http.HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

	public const string CachePath = "user://cache";

	private static App _instance;

	private App()
	{
		CreateDirs();
	}

	public static void Initialize()
	{
		if (_instance is not null) return;
		
		GD.PrintRich($"[b]Nasara v{GetVersion()}[/b]. Licensed under the MIT License.\n" +
		             "Check out our GitHub: https://github.com/nofuncoding/nasara\n");
		Log("Initializing");
		_instance = new App();
	}

	public static App Get() => _instance;
	
	/// <summary>
	/// Get the current version of app
	/// </summary>
	/// <returns>Version string</returns>
	public static string GetVersion() => (string)ProjectSettings.GetSetting("application/config/version");

	/// <summary>
	/// A simple log function
	/// </summary>
	/// <param name="message">Text to log</param>
	/// <param name="identifier">Identifier of log message</param>
	/// <param name="richEnable">Enable rich support in terminal</param>
	public static void Log(string message, string identifier="App", bool richEnable=false)
	{
		// TODO ugly. why not rewrite it?
		// var time = new DateTime().AddMilliseconds(Time.GetTicksMsec());
		var time = DateTime.Now;
		var timeString = $"{time:h:mm:ss.fff}";
		if (richEnable) GD.PrintRich($"[{timeString}] ({identifier}) {message}");
		else GD.Print($"[{timeString}] ({identifier}) {message}");
	}
	
	private static void CreateDirs()
	{
		// The directories need to create when starting
		string[] dirs = [CachePath];

		foreach (var d in dirs)
		{
			// If not exists, then create it.
			if (!DirAccess.DirExistsAbsolute(d))
			{
				DirAccess.MakeDirRecursiveAbsolute(d);
			}
		}
	}
}
