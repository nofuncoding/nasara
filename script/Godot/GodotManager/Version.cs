using System;
using Semver;
using Godot;

namespace Nasara.GodotManager;

public partial class Version : Node
{
    public Godot.Collections.Array<GodotVersion> GetVersions()
    {
        VersionReader reader = new();
        return reader.Read();
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
            using var file = FileAccess.Open(VersionReader.ListPath, FileAccess.ModeFlags.Write);
            foreach (GodotVersion ver in checkedVersions)
                WriteVersion(file, ver);
        } else {
            using var file = FileAccess.Open(VersionReader.ListPath, FileAccess.ModeFlags.ReadWrite);
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

    public void RemoveVersion(GodotVersion godotVersion)
    {
        Godot.Collections.Array<GodotVersion> versionsBeforeRemoval = GetVersions();
        Godot.Collections.Array<GodotVersion> versions = new();

        foreach (GodotVersion ver in versionsBeforeRemoval)
            if (ver.Version != godotVersion.Version)
                versions.Add(ver);
            else
                if (ver.Mono != godotVersion.Mono)
                    versions.Add(ver);

        using var file = FileAccess.Open(VersionReader.ListPath, FileAccess.ModeFlags.Write);
        foreach (GodotVersion version in versions)
            WriteVersion(file, version);
        GD.Print($"Removed {godotVersion.Version}");

        GetNode<App>("/root/App").GetNotifySystem().Notify(
            title: Tr("Editor Deleted"),
            description: string.Format(Tr("Deleted Godot {0}"), godotVersion.Version),
            type: UI.NotificationType.Warn
        );
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
    
    /// <summary>
    /// Return the version when path has Godot installation
    /// </summary>
    /// <param name="path">The absolute path</param>
    /// <returns>null if not available</returns>
    public static GodotVersion PathHasGodot(string path)
    {
        if (!DirAccess.DirExistsAbsolute(path))
            return null;

        bool available = false;
        bool mono = false;
        GodotVersion.VersionChannel channel = GodotVersion.VersionChannel.Stable;
        SemVersion version = null;

        DirAccess dirAccess = DirAccess.Open(path);
        if (dirAccess is null)
        {
            // GD.PushError($"{DirAccess.GetOpenError()}:{path}");
            return null;
        }

        foreach (string filename in dirAccess.GetFiles())
        {
            string[] fn = filename.Split("_");
            if (filename.GetExtension() == "exe" && fn[0] == "Godot")
            {
                available = true;
                if (fn[2] == "mono")
                    mono = true;
                
                string v = fn[1].TrimSuffix("-stable");
                version = SemVersion.Parse(v, SemVersionStyles.Any);

                if (version.ComparePrecedenceTo(new SemVersion(3)) == -1) // Ignoring Versions Before 3
                    return null;

                if (version.IsPrerelease)
                    channel = GodotVersion.VersionChannel.Unstable;

                break;
            }
        }

        if (!available)
            return null;
        
        GodotVersion result = new(version, path, channel, mono);
        return result;
    }
}

class VersionReader
{
    public static readonly string ListPath = "user://gdls";

    /// <summary>
    /// Process legacy version
    /// </summary>
    /// <returns></returns>
    bool LegacyProcess() // maybe dont need return
    {
        if (FileAccess.FileExists("user://godot_list")) // v0.1.0-beta.1
        {
            if (DirAccess.CopyAbsolute("user://godot_list", ListPath) == Error.Ok)
                DirAccess.RemoveAbsolute("user://godot_list");
            else
                throw new Exception("Failed to delete old file");
        } else {
            return false;
        }
        GD.Print("[VersionListReader] Found Old Version List. Translated.");

        return true;
    }

    Godot.Collections.Array<GodotVersion> ReadFile(FileAccess file)
    {
        Godot.Collections.Array<GodotVersion> godotVersions = new();

        
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

        file.Close();

        return godotVersions;
    }

    public Godot.Collections.Array<GodotVersion> Read()
    {
        LegacyProcess();

        if (!FileAccess.FileExists(ListPath))
            FileAccess.Open(ListPath, FileAccess.ModeFlags.Write); // Creating File

        using var file = FileAccess.Open(ListPath, FileAccess.ModeFlags.Read);
        if (file is not null)
        {
            Godot.Collections.Array<GodotVersion> godotVersions = ReadFile(file);

            if (godotVersions.Count > 0)
                return godotVersions;
            else if (godotVersions.Count == 0)
            {
                GD.PushWarning("No Godot Versions Found!");
                return godotVersions;
            }
        }
        GD.PushError("Could not get installed godot versions: ", FileAccess.GetOpenError());
        return new();
    }
}
