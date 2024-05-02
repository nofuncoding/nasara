using Godot;
using System;
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

	private App()
	{
		CreateDirs();
		NetworkClient.Initialize();
		_notificationSystemInstance = new NotificationSystem();
	}

	public static void Initialize(AppLayout layout)
	{
		if (_instance is not null) return;
		
		GD.PrintRich($"[b]Nasara v{GetVersion()}[/b] by NoFun\n" +
		             "https://github.com/nofuncoding/nasara\n");
		
		Logger.Log("Initializing");
		Logger.LogWarn("Testing");
		_instance = new App();
		_layoutInstance = layout;
	}

	public static App Get() => _instance;
	public static AppLayout GetLayout() => _layoutInstance;
	
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


public static class Logger
{
	/// <summary>
	/// A simple log function
	/// </summary>
	/// <param name="message">Text to log</param>
	/// <param name="identifier">Identifier of log message</param>
	public static void Log(string message, string identifier="App")
	{
		GD.PrintRich(GenerateLogMessage(identifier, message, LogLevel.Info));
	}

	public static void LogWarn(string message, string identifier="App")
	{
		if (OS.IsDebugBuild())
			GD.PushWarning(GenerateLogMessage(identifier, message, LogLevel.Warn, false));
		else
			GD.PrintRich(GenerateLogMessage(identifier, message, LogLevel.Warn));
	}

	public static void LogError(string message, string identifier = "App")
	{
		if (OS.IsDebugBuild())
			GD.PushError(GenerateLogMessage(identifier, message, LogLevel.Error, false));
		else
			GD.PrintRich(GenerateLogMessage(identifier, message, LogLevel.Error));
	}

	private static string GenerateLogMessage(string identifier, string message, LogLevel level, bool enableRich=true)
	{
		// var time = new DateTime().AddMilliseconds(Time.GetTicksMsec());
		var time = DateTime.Now;
		var timeString = $"{time:HH:mm:ss.fff}";
		if (enableRich)
			return level switch
			{
				LogLevel.Info => $"[{timeString}] ({identifier}) [b]INFO[/b] {message}",
				LogLevel.Warn => $"[color=yellow][{timeString}] ({identifier}) [b]WARN[/b] {message}[/color]",
				LogLevel.Error => $"[color=red][{timeString}] ({identifier}) [b]ERROR[/b] {message}[/color]",
				LogLevel.Fatal => $"[color=red][{timeString}] ({identifier}) [b]FATAL[/b] {message}[/color]",
				LogLevel.Debug => $"[color=grey][{timeString}] ({identifier}) [b]DEBUG[/b] {message}[/color]",
				_ => $"[{timeString}] ({identifier}) [b]???? [/b] {message}",
			};
		else
			return level switch
			{
				LogLevel.Info => $"[{timeString}] ({identifier}) INFO  {message}",
				LogLevel.Warn => $"[{timeString}] ({identifier}) WARN  {message}",
				LogLevel.Error => $"[{timeString}] ({identifier}) ERROR {message}",
				LogLevel.Fatal => $"[{timeString}] ({identifier}) FATAL {message}",
				LogLevel.Debug => $"[{timeString}] ({identifier}) DEBUG {message}",
				_ => $"[{timeString}] ({identifier}) ????  {message}",
			};
	}
	
	public enum LogLevel
	{
		Info,
		Warn,
		Error,
		Fatal,
		Debug,
	}
}