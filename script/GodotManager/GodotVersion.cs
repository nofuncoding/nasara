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
    public VersionStatus Status { get; }

    public enum VersionChannel
    {
        Stable,
        Unstable
    }

    public enum VersionStatus
    {
        OK,
        NotFound,
    }

    // You should use a dir path
    public GodotVersion(string version, string path, VersionChannel channel=VersionChannel.Stable, bool mono=false)
    {
        Version = SemVersion.Parse(version, SemVersionStyles.Strict);
        Path = path;
        Status = VersionStatus.OK;

        if (!DirAccess.DirExistsAbsolute(path))
        {
            // GD.PushError("Godot Installation Not Found: `", path, "` is Unreachable");
            Status = VersionStatus.NotFound;
        }

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

