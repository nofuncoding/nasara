using Godot;

namespace Nasara;

// TODO waiting for rewrite
public partial class AppConfig : RefCounted
{
    const string ConfigPath = "user://nasara.cfg";
    private ConfigFile _configFile;

    /* App */
    public string Language { get => (string)GetValue("language");
        set => SetValue("language", value, needRestart: true);
    }
    /* Network */
    public bool UsingGithubProxy { get => (bool)GetValue("github_proxy", false, "network");
        set => SetValue("github_proxy", value, "network");
    }
    /* Editor */
    public bool OpenEditorConsole { get => (bool)GetValue("open_console", false, "editor");
        set => SetValue("open_console", value, "editor");
    }

    [Signal]
    public delegate void NeedRestartEventHandler();

    public AppConfig()
    {
        // TODO: Use a cache in memory to avoid reloading the config file every time
        _configFile = new ConfigFile();
        
        if (Load() != Error.Ok)
            Save();
    }

    private Variant GetValue(string key, Variant @default = default, string section="app")
    {
        return _configFile.GetValue(section, key, @default);
    }

    private void SetValue(string key, Variant value, string section="app", bool needRestart = false)
    {
        _configFile.SetValue(section, key, value);
        if (Save() != Error.Ok) // Save config
            GD.PrintErr("(config) Cannot save config: ", key);
        
        if (needRestart)
            EmitSignal(SignalName.NeedRestart);
        
        GD.Print($"(config) Set {section}/{key} to {value}");
    }

    private Error Save()
    {
        return _configFile.Save(ConfigPath);
    }

    private Error Load()
    {
        return _configFile.Load(ConfigPath);
    }
}
