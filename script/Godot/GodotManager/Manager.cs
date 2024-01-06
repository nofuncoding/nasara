using Godot;
using System;
using Semver;

namespace Nasara.GodotManager {
    // Just a pack for ref.
    public partial class Manager : Node
    {
        Requester requester;
        Version version;

        public override void _Ready()
        {
            requester = new();
            version = new();

            AddChild(requester);
            AddChild(version);
        }

        public Requester Requester() // why need it
        {
            return requester;
        }

        public Version Version() // and this?
        {
            return version;
        }

    
        public void Launch(GodotVersion version)
        {
            Launcher launcher = new(version);
            AddChild(launcher);
            launcher.Launch(); // Free after launch
        }
    }
}