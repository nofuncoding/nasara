using Godot;
using System;
using Semver;

namespace Nasara.Core.Management.Editor;

[GlobalClass] // Removable?
public partial class GodotVersion : RefCounted
{
    // TODO: Add Platform

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

    /// <summary>
    /// Build new editor version
    /// </summary>
    /// <param name="version">Editor version</param>
    /// <param name="path">The path to the installation directory</param>
    /// <param name="channel">Stable or Unstable</param>
    /// <param name="mono">Is mono?</param>
    public GodotVersion(SemVersion version, string path, VersionChannel channel=VersionChannel.Stable, bool mono=false)
    {
        Version = version;
        Path = ProjectSettings.GlobalizePath(path);
        if (!DirAccess.DirExistsAbsolute(path))
            GD.PushError("Godot Installation Not Found: `", path, "` is Unreachable");

        Channel = channel;
        Mono = mono;
    }
}
