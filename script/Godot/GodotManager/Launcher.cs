using System;
using System.Security.AccessControl;
using Godot;

namespace GodotManager {
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
        /// Launch the given version. Will self delete after launching.
        /// </summary>
        /// <returns></returns>
        public Error Launch()
        {
            DirAccess dirAccess = DirAccess.Open(Version.Path);
            if (dirAccess is null) // Error
                return DirAccess.GetOpenError();

            foreach (string filename in dirAccess.GetFiles())
            {
                if (filename.GetExtension() == "exe" && filename.Contains("Godot"))
                {
                    string executablePath = Version.Path.PathJoin(filename);
                    GD.Print($"Running Godot {Version.Version}; Mono={Version.Mono}\nPath: {ProjectSettings.GlobalizePath(executablePath)}");
                    string[] argument = {"--project-manager"}; // Run in Project Manager
                    _ = OS.CreateProcess(ProjectSettings.GlobalizePath(executablePath), argument, new AppConfig().OpenEditorConsole); // Open a native OS path

                    GetNode<App>("/root/App").GetNotifySystem().Notify(title: Tr("Editor Launched"), description: string.Format(Tr("Launched Godot {0}"), Version.Version));
                    QueueFree();
                    return Error.Ok;
                }
            }
            QueueFree();
            return Error.FileNotFound;
        }
    }
}
