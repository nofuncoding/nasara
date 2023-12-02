using Godot;
using System;

public partial class AppConfig : RefCounted
{

    // TODO: More sections

    static string configPath = "user://nasara.cfg";
    ConfigFile configFile;

    // public bool TestVar { get { return (bool)GetValue("test", false); }  set { SetValue("test", value); } }

    // TODO: add option to enable TLS
    public bool EnableTLS { get { return (bool)GetValue("enable_tls", false); }  set { SetValue("enable_tls", value); } }
    public bool OpenEditorConsole { get { return (bool)GetValue("open_console", false, "editor"); }  set { SetValue("open_console", value, "editor"); } }

    public AppConfig()
    {
        configFile = new ConfigFile();
        
        if (Load() != Error.Ok)
            Save();
    }

    Variant GetValue(string key, Variant @default = default, string section="app")
    {
        return configFile.GetValue(section, key, @default);
    }

    void SetValue(string key, Variant value, string section="app")
    {
        configFile.SetValue(section, key, value);
        if (Save() != Error.Ok)
            GD.PrintErr("Cannot save config: ", key);
    }

    Error Save()
    {
        return configFile.Save(configPath);
    }

    Error Load()
    {
        return configFile.Load(configPath);
    }
}
