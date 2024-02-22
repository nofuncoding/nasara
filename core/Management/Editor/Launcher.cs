using System;
using System.Diagnostics;
using System.Threading;
using Godot;

namespace Nasara.Core.Management.Editor;

public partial class Launcher : Node
{
    GodotVersion Version;

    public Launcher(GodotVersion version)
    {
        if (DirAccess.DirExistsAbsolute(version.Path))
            Version = version;
        else
            throw new Exception($"The given path `{version.Path}` is invaild");
    }

    /// <summary>
    /// Launch the given version. The launcher will self delete after launching.
    /// </summary>
    /// <returns></returns>
    public Error Launch()
    {
        UI.NotifySystem notifySystem = App.GetNotifySystem();

        using var dirAccess = DirAccess.Open(Version.Path);
        if (dirAccess is null) // Error
            return DirAccess.GetOpenError();

        foreach (string filename in dirAccess.GetFiles())
        {
            if (filename.GetExtension() == "exe" && filename.Contains("Godot"))
            {
                string executablePath = Version.Path.PathJoin(filename);
                executablePath = ProjectSettings.GlobalizePath(executablePath);
                GD.Print($"Running Godot {Version.Version}; Mono={Version.Mono}\nPath: {ProjectSettings.GlobalizePath(executablePath)}");
                string[] arguments = ["-p"]; // Launch in project manager
                // Create a process
                var pid = OS.CreateProcess(executablePath, arguments, new AppConfig().OpenEditorConsole);

                // Wait for process to launch
                Thread.Sleep(100);

                var process = Process.GetProcessById(pid);

                // TODO: The notification should not be managed in the backend
                if (!process.HasExited)
                {
                    notifySystem.Notify(title: Tr("Editor Launched"), description: string.Format(Tr("Launched Godot {0}"), Version.Version));
                    GD.Print($"Running Godot {Version.Version}; Mono={Version.Mono}\nPath: {executablePath}");
                }
                else
                {
                    notifySystem.Notify(title: Tr("Failed to launch editor"),
                                        description: string.Format(Tr("Failed to launched Godot {0}, error code: {1}"), Version.Version, process.ExitCode));
                }
                QueueFree();
                return Error.Ok;
            }
        }
        QueueFree();
        return Error.FileNotFound;
    }

    public Error Launch(string projectDirectory)
    {
        UI.NotifySystem notifySystem = App.GetNotifySystem();

        using var dirAccess = DirAccess.Open(Version.Path);
        if (dirAccess is null) // Error
            return DirAccess.GetOpenError();

        foreach (string filename in dirAccess.GetFiles())
        {
            if (filename.GetExtension() == "exe" && filename.Contains("Godot"))
            {
                string executablePath = Version.Path.PathJoin(filename);
                executablePath = ProjectSettings.GlobalizePath(executablePath);

                string[] arguments = ["-e", "--path", projectDirectory]; // Launch in project manager

                // Create a process
                var pid = OS.CreateProcess(executablePath, arguments, new AppConfig().OpenEditorConsole);

                // Wait for process to launch
                Thread.Sleep(100);

                var process = Process.GetProcessById(pid);

                // TODO: The notification should not be managed in the backend
                if (!process.HasExited)
                {
                    notifySystem.Notify(title: Tr("Editor Launched"), description: string.Format(Tr("Launched Godot {0}"), Version.Version));
                    GD.Print($"Running Godot {Version.Version}; Mono={Version.Mono}\nPath: {executablePath}");
                }
                else
                {
                    notifySystem.Notify(title: Tr("Failed to launch editor"),
                                        description: string.Format(Tr("Failed to launched Godot {0}, error code: {1}"), Version.Version, process.ExitCode));
                }
                QueueFree();
                return Error.Ok;
            }
        }
        QueueFree();
        return Error.FileNotFound;
    }
}

