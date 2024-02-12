using Godot;
using System;
using Semver;

namespace Nasara.Core.Management.Editor;

/// <summary>
/// The main class of the GodotManager.
/// Contains requester and version references.
/// </summary>
public partial class Manager : Node
{
    public Requester Requester { get; private set; }
    public Version Version { get; private set; }

    public override void _Ready()
    {
        Requester = new();
        Version = new();

        AddChild(Requester);
        AddChild(Version);
    }

    public void Launch(GodotVersion version)
    {
        Launcher launcher = new(version);
        AddChild(launcher);
        launcher.Launch(); // Free after launch
    }
}
