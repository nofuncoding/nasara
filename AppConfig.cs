using Godot;

namespace Nasara;

public partial class AppConfig : RefCounted
{
    const string CONFIG_PATH = "user://nasara.cfg";
    ConfigFile configFile;

    /* App */
    public string Language { get { return (string)GetValue("language"); } set { SetValue("language", value, needRestart: true);} }
    /* Network */
    public bool EnableTLS { get { return (bool)GetValue("enable_tls", false, "network"); }  set { SetValue("enable_tls", value, "network"); } }
    public bool UsingGithubProxy { get { return (bool)GetValue("github_proxy", false, "network"); }  set { SetValue("github_proxy", value, "network"); } }
    /* Theme */
    public bool TransparentBackground { get { return (bool)GetValue("transparent_background", true, "theme"); }  set { SetValue("transparent_background", value, "theme"); } }
    public bool UseCustomTitlebar { get { return (bool)GetValue("use_custom_titlebar", true, "theme"); }  set { SetValue("use_custom_titlebar", value, "theme", needRestart: true); } }
    /* Editor */
    public bool OpenEditorConsole { get { return (bool)GetValue("open_console", false, "editor"); }  set { SetValue("open_console", value, "editor"); } }

    [Signal]
    public delegate void NeedRestartEventHandler();

    public AppConfig()
    {
        // TODO: Use a cache in memory to avoid reloading the config file every time
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
        return configFile.Save(CONFIG_PATH);
    }

    Error Load()
    {
        return configFile.Load(CONFIG_PATH);
    }
}
