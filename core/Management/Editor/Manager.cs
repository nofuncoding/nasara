using Godot;
using System;
using Semver;

namespace Nasara.Core.Management.Editor;

// TODO: refactor all this, hide all the low-level logic instead of
//   let the user process it, and pass the status with the events.
//   also need to do it in the projects manager.

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
