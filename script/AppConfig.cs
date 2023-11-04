using Godot;
using System;

public partial class AppConfig : Node
{
    public ConfigFile config;

    string config_path = "user://nasara.cfg";

    public override void _Ready()
    {
        config = new ConfigFile();

        if (config.Load(config_path) == Error.FileNotFound)
            config.Save(config_path);
    }

    public AppConfig Get()
    {
        return GetTree().Root.GetNode<AppConfig>("AppConfig");
    }
}
