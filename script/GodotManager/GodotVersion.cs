using Godot;
using System;
using Semver;

[GlobalClass] // Removable?
public partial class GodotVersion : RefCounted
{
    public VersionChannel Channel { get; }
    public SemVersion Version { get; }
    public bool Mono { get; }
    public string Path { get; }

    public enum VersionChannel
    {
        Stable,
        Unstable
    }

    // You should use a dir path
    public GodotVersion(string version, string path, VersionChannel channel=VersionChannel.Stable, bool mono=false)
    {
        Version = SemVersion.Parse(version, SemVersionStyles.Strict);
        Path = path;
        if (!DirAccess.DirExistsAbsolute(path))
            GD.PushError("Godot Installation Not Found: `", path, "` is Unreachable");

        Channel = channel;
        Mono = mono;
    }

    public GodotVersion(SemVersion version, string path, VersionChannel channel=VersionChannel.Stable, bool mono=false)
    {
        Version = version;
        Path = path;
        if (!DirAccess.DirExistsAbsolute(path))
            GD.PushError("Godot Installation Not Found: `", path, "` is Unreachable");

        Channel = channel;
        Mono = mono;
    }
}

