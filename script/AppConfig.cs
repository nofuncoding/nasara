using Godot;
using System;

public partial class AppConfig : RefCounted
{

    // TODO: More sections

    static string configPath = "user://nasara.cfg";
    ConfigFile configFile;

    public bool TestVar { get { return (bool)GetValue("test", false); }  set { SetValue("test", value); } }

    public AppConfig() {
        configFile = new ConfigFile();
        
        if (Load() != Error.Ok)
            Save();
    }

    Variant GetValue(string key, Variant @default = default) {
        return configFile.GetValue("app", key, @default);
    }

    void SetValue(string key, Variant value) {
        configFile.SetValue("app", key, value);
        if (Save() != Error.Ok)
            GD.PrintErr("Cannot save config: ", key);
    }

    Error Save() {
        return configFile.Save(configPath);
    }

    Error Load() {
        return configFile.Load(configPath);
    }
}
