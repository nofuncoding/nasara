using Godot;
using Semver;
using System;

public partial class GodotManager : Node
{
    public static string godotListPath = "user://godot_list";
    // Godot.Collections.Dictionary<string, string> InstalledGodot = new();

    GodotRequester godotRequester;

    public override void _Ready()
    {
        godotRequester = new GodotRequester();
        AddChild(godotRequester);
        
        if (!FileAccess.FileExists(godotListPath))
            FileAccess.Open(godotListPath, FileAccess.ModeFlags.Write); // Creating File
    }

    public GodotRequester GetRequester()
    {
        return godotRequester;
    }

    public Godot.Collections.Array<GodotVersion> GetVersions()
    {
        if (!FileAccess.FileExists(godotListPath))
            FileAccess.Open(godotListPath, FileAccess.ModeFlags.Write); // Creating File

        /*
        FIXME: Error when file full of spaces
        E 0:00:02:0847   Godot.Collections.Array`1[GodotVersion] GodotManager.GetVersions(): System.IndexOutOfRangeException: Index was outside the bounds of the array.
        <C# 错误>        System.IndexOutOfRangeException
        <C# 源文件>      GodotManager.cs:42 @ Godot.Collections.Array`1[GodotVersion] GodotManager.GetVersions()
        <栈追踪>         GodotManager.cs:42 @ Godot.Collections.Array`1[GodotVersion] GodotManager.GetVersions()
                         GodotManager.cs:12 @ void GodotManager._Ready()
                         Node.cs:2117 @ bool Godot.Node.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
                         GodotManager_ScriptMethods.generated.cs:88 @ bool GodotManager.InvokeGodotClassMethod(Godot.NativeInterop.godot_string_name&, Godot.NativeInterop.NativeVariantPtrArgs, Godot.NativeInterop.godot_variant&)
                         CSharpInstanceBridge.cs:24 @ Godot.NativeInterop.godot_bool Godot.Bridge.CSharpInstanceBridge.Call(nint, Godot.NativeInterop.godot_string_name*, Godot.NativeInterop.godot_variant**, int, Godot.NativeInterop.godot_variant_call_error*, Godot.NativeInterop.godot_variant*)

        */

        using var file = FileAccess.Open(godotListPath, FileAccess.ModeFlags.Read);
        if (file is not null)
        {
            Godot.Collections.Array<GodotVersion> godotVersions = new();

            while (file.GetPosition() < file.GetLength())
            {
                bool isMono = false;

                string rawLine = file.GetLine();

                string[] s0 = rawLine.Split("@", false);
                string[] s1 = s0[1].Split("=", false);

                string channel;

                if (s0[0].Contains("[mono]"))
                {
                    isMono = true;
                    channel = s0[0].TrimPrefix("[mono]");
                }
                else
                    channel = s0[0];
                string version = s1[0];
                string path = s1[1];

                if (version.Length > 0 && path.Length > 0 && (channel == "stable" || channel == "unstable"))
                {
                    if (channel == "unstable")
                        godotVersions.Add(new GodotVersion(version, path, GodotVersion.VersionChannel.Unstable, isMono));
                    else
                        godotVersions.Add(new GodotVersion(version, path, GodotVersion.VersionChannel.Stable, isMono));
                } else {
                    GD.PushError("Failed to Read `godot_list`: Invaild file style");
                }
            }

            return godotVersions;
        } else {
            GD.PushError("Failed to Read `godot_list`: ", FileAccess.GetOpenError());
            return new();
        }
    }
    
    public void AddVersion(GodotVersion godotVersion)
    {
        if (VersionExists(godotVersion)) // FIXME: Bad coding, need refactor
        {
            
            Godot.Collections.Array<GodotVersion> versions = GetVersions();
            Godot.Collections.Array<GodotVersion> checkedVersions = new();
            foreach (GodotVersion ver in versions)
            {
                if (ver.Version == godotVersion.Version)
                {
                    // Ignoring Version Exists
                    if (ver.Path == godotVersion.Path)
                        return;
                    else {
                        // Modfiy Path
                        checkedVersions.Add(godotVersion);
                    }
                } else
                    checkedVersions.Add(ver);
            }

            // Fully Clear and Rewrite
            using var file = FileAccess.Open(godotListPath, FileAccess.ModeFlags.Write);
            foreach (GodotVersion ver in checkedVersions)
            {
                WriteVersion(file, ver);
            }
        } else {
            using var file = FileAccess.Open(godotListPath, FileAccess.ModeFlags.ReadWrite);
            WriteVersion(file, godotVersion);
        }
    }

    private void WriteVersion(FileAccess file, GodotVersion godotVersion)
    {
        /* Contents in `godot_list` will like this:
        [mono](channel)@(version)=(path)
        e.g.
        stable@4.1.1=C:/Godot/4.1.1
        [mono]unstable@4.2-rc1=C:/Godot/4.2-rc1

        channel must be `stable` or `unstable`

        TODO: Change the whole manager because it's shit
        */

        string channel_str = "";

        switch (godotVersion.Channel)
        {
            case GodotVersion.VersionChannel.Stable:
                channel_str = "stable";
                break;
            case GodotVersion.VersionChannel.Unstable:
                channel_str = "unstable";
                break;
        }

        file.SeekEnd();
        if (godotVersion.Mono)
            file.StoreLine($"[mono]{channel_str}@{godotVersion.Version}={godotVersion.Path}");
        else
            file.StoreLine($"{channel_str}@{godotVersion.Version}={godotVersion.Path}");
        // file.Dispose();
    }

    public bool VersionExists(string version)
    {
        Godot.Collections.Array<GodotVersion> versions = GetVersions();
        foreach (GodotVersion ver in versions)
            if (ver.Version.Equals(SemVersion.Parse(version, SemVersionStyles.Strict))) // ugly
                return true;
        return false;
    }

    public bool VersionExists(GodotVersion version)
    {
        Godot.Collections.Array<GodotVersion> versions = GetVersions();
        foreach (GodotVersion ver in versions)
            if (ver.Version == version.Version && ver.Mono == version.Mono) // Wtf
                return true;
        return false;
    }

    public Error LaunchVersion(GodotVersion version)
    {
        DirAccess dirAccess = DirAccess.Open(version.Path);
        if (dirAccess is null) // Error
            return DirAccess.GetOpenError();

        foreach (string filename in dirAccess.GetFiles())
        {
            if (filename.GetExtension() == "exe" && filename.Contains("Godot"))
            {

                string executablePath = version.Path.PathJoin(filename);
                GD.Print($"Running Godot {version.Version}; Mono={version.Mono}\nPath: {ProjectSettings.GlobalizePath(executablePath)}");
                string[] argument = {"--project-manager"}; // Run in Project Manager
                int pid = OS.CreateProcess(ProjectSettings.GlobalizePath(executablePath), argument, new AppConfig().OpenEditorConsole); // Open a native OS path
                
                GetNode<App>("/root/App").GetNotifySystem().Notify(title: "Editor Launched", description: $"Launched Godot {version.Version}");
                return Error.Ok;
            }
        }
        
        return Error.FileNotFound;
    }
}
