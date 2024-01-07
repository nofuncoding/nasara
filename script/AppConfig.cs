using Godot;
using System;

namespace Nasara;

public partial class AppConfig : RefCounted
{
    static string configPath = "user://nasara.cfg";
    ConfigFile configFile;

    // public bool TestVar { get { return (bool)GetValue("test", false); }  set { SetValue("test", value); } }

    // TODO: add option to enable TLS
    public string Language { get { return (string)GetValue("language", ""); } set { SetValue("language", value, needRestart: true);} }
    public bool EnableTLS { get { return (bool)GetValue("enable_tls", false, "network"); }  set { SetValue("enable_tls", value, "network"); } }
    public bool UsingGithubProxy { get { return (bool)GetValue("github_proxy", false, "network"); }  set { SetValue("github_proxy", value, "network"); } }
    public bool OpenEditorConsole { get { return (bool)GetValue("open_console", false, "editor"); }  set { SetValue("open_console", value, "editor"); } }

    [Signal]
    public delegate void NeedRestartEventHandler();

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

    void SetValue(string key, Variant value, string section="app", bool needRestart = false)
    {
        configFile.SetValue(section, key, value);
        if (Save() != Error.Ok) // Save config
            GD.PrintErr("(config) Cannot save config: ", key);
        
        if (needRestart)
            EmitSignal(SignalName.NeedRestart);
        
        GD.Print($"(config) Set {section}/{key} to {value}");
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
