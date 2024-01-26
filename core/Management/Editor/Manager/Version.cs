using System;
using Semver;
using Godot;
using System.Linq;
using Godot.Collections;
using System.Threading.Tasks.Dataflow;

namespace Nasara.Core.Management.Editor;

public partial class Version : Node
{
    public Array<GodotVersion> GetVersions()
    {
        return VersionWriter.Read();
    }
    
    public void AddVersion(GodotVersion godotVersion)
    {
        if (VersionExists(godotVersion)) // FIXME: Bad coding, need refactor
        {
            Array<GodotVersion> versions = GetVersions();
            Array<GodotVersion> checkedVersions = new();

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

            VersionWriter.Write(checkedVersions);
        } else {
            Array<GodotVersion> versions = GetVersions();
            versions.Add(godotVersion);
            VersionWriter.Write(versions);
        }
    }

    public void RemoveVersion(GodotVersion godotVersion)
    {
        Array<GodotVersion> versionsBeforeRemoval = GetVersions();
        Array<GodotVersion> versions = new();

        foreach (GodotVersion ver in versionsBeforeRemoval)
            if (ver.Version != godotVersion.Version)
                versions.Add(ver);
            else
                if (ver.Mono != godotVersion.Mono)
                    versions.Add(ver);

        VersionWriter.Write(versions);
        GD.Print($"Removed {godotVersion.Version}");

        GetNode<App>("/root/App").GetNotifySystem().Notify(
            title: Tr("Editor Deleted"),
            description: string.Format(Tr("Deleted Godot {0}"), godotVersion.Version),
            type: UI.NotificationType.Warn
        );
    }

    public bool VersionExists(string version)
    {
        Array<GodotVersion> versions = GetVersions();
        foreach (GodotVersion ver in versions)
            if (ver.Version.Equals(SemVersion.Parse(version, SemVersionStyles.Strict))) // ugly
                return true;
        return false;
    }

    public bool VersionExists(GodotVersion version)
    {
        Array<GodotVersion> versions = GetVersions();
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

class VersionReaderLegacy
{
    public static Array<GodotVersion> ReadFile(FileAccess file)
    {
        Array<GodotVersion> godotVersions = new();
        
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

}

class VersionWriter
{
    public const string EDITORLIST = "user://editors.json";

    /*Example:
    [
        {
            "version": "4.0",
            "path": "X:/xx/editors/Godot-4.0",
            "mono": false,
            "unstable": false,
        },
        ...
    ]*/

    public static Error Write(Array<GodotVersion> godotVersions)
    {
        Array<Dictionary> version_data = new();
        foreach (var ver in godotVersions)
        {
            Dictionary verData = new();
            verData["version"] = ver.Version.ToString();
            verData["path"] = ver.Path;
            verData["mono"] = ver.Mono;
            verData["unstable"] = ver.Channel == GodotVersion.VersionChannel.Unstable;
            version_data.Add(verData);
        }

        string data = Json.Stringify(version_data);
        using var file = FileAccess.Open(EDITORLIST, FileAccess.ModeFlags.Write);
        if (file is null)
            return FileAccess.GetOpenError();
        
        file.StoreString(data);
        return Error.Ok;
    }

    static Array<GodotVersion> ReadFile(FileAccess file)
    {
        if (file.GetLength() == 0) // Empty file
            return new();

        Json json = new();
        var error = json.Parse(file.GetAsText());
        if (error != Error.Ok)
        {
            GD.PushError($"Failed to Parse {file.GetPath()}: {error}");
            return new();
        }

        var data = (Godot.Collections.Array)json.Data;
        Array<GodotVersion> versions = new();
        foreach (Dictionary ver in data.Select(v => (Dictionary)v))
        {
            SemVersion semVersion = SemVersion.Parse((string)ver["version"], SemVersionStyles.Any);
            string path = (string)ver["path"];
            bool mono = (bool)ver["mono"];
            GodotVersion.VersionChannel channel = (bool)ver["unstable"] ? GodotVersion.VersionChannel.Unstable : GodotVersion.VersionChannel.Stable;
        
            GodotVersion godotVersion = new(semVersion, path, channel, mono);
            versions.Add(godotVersion);
        }

        return versions;
    }

    static Array<GodotVersion> ProcessLegacy()
    {
        string path = FileAccess.FileExists("user://gdls") ? "user://gdls" : "user://godot_list";
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        
        if (file is null)
            return new();
        
        var data = VersionReaderLegacy.ReadFile(file); // Read from legacy
        Write(data); // Write to new

        DirAccess.RemoveAbsolute(path); // Remove legacy

        return data;
    }

    public static Array<GodotVersion> Read()
    {

        if (!FileAccess.FileExists(EDITORLIST))
        {
            // Creating File
            FileAccess.Open(EDITORLIST, FileAccess.ModeFlags.Write);

            if (FileAccess.FileExists("user://gdls") || FileAccess.FileExists("user://godot_list"))
                return ProcessLegacy();

            return new();
        }

        using var file = FileAccess.Open(EDITORLIST, FileAccess.ModeFlags.Read);
        if (file is not null)
        {
            Array<GodotVersion> godotVersions = ReadFile(file);
            if (godotVersions.Count == 0) // TODO: Remove or expand this
                GD.PushWarning("No Godot Versions Found!");
            
            return godotVersions;
        }

        // file is null
        GD.PushError("Could not get installed godot versions: ", FileAccess.GetOpenError());
        return new();
    }
}