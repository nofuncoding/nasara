using Godot;
using System;
using System.Diagnostics;

namespace Nasara;

public static class Logger
{
	public static void Log(string message)
	{
		GD.PrintRich(GenerateLogMessage(GetIdentifier(), message, LogLevel.Info));
	}

	public static void LogWarn(string message)
	{
		if (OS.IsDebugBuild())
			GD.PushWarning(GenerateLogMessage(GetIdentifier(), message, LogLevel.Warn, false));
		else
			GD.PrintRich(GenerateLogMessage(GetIdentifier(), message, LogLevel.Warn));
	}

	public static void LogError(string message)
	{
		if (OS.IsDebugBuild())
			GD.PushError(GenerateLogMessage(GetIdentifier(), message, LogLevel.Error, false));
		else
			GD.PrintRich(GenerateLogMessage(GetIdentifier(), message, LogLevel.Error));
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

	private static string GetIdentifier()
	{
		// TODO bad code
		// Get stack to find who logged
		var stackTrace = new StackTrace();
		// Get the third frame
		var method = stackTrace.GetFrame(2)?.GetMethod();
		
		var logClass = method?.DeclaringType;
		if ((logClass?.IsClass ?? false) && !method.IsStatic)
			logClass = logClass.DeclaringType;
		
		var identifier = logClass?.Name;
		identifier ??= method?.ReflectedType?.Name; // FIXME
		identifier ??= "Unknown";

		return identifier;
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