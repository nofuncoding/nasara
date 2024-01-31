using Godot;
using Semver;
using System;

namespace Nasara.Core.Management.Project;

// TODO: I dont know how the godot engine process it...
// Maybe it will check it later.

/// <summary>
/// Reads the godot project file `project.godot`
/// </summary>
public partial class ProjectFile : RefCounted
{
    public readonly string Path;
    
    /// <summary>
    /// I found the `ConfigFile` can read project file, so that's it!
    /// </summary>
    ConfigFile file;

    public ProjectFile(string path)
    {
        if (path.GetFile() != "project.godot")
            throw new Exception("Invalid project file path");
    
        Path = path;

        // load the file
        file = new();
        var err = file.Load(Path);

        if (err != Error.Ok)
            throw new Exception("Invalid project file");
        
        int config_ver = (int)file.GetValue("", "config_version");
        if (config_ver < 5)
            throw new Exception($"Project is not supported (config_version={config_ver})");
    }

    public string GetProjectName()
    {
        return (string)file.GetValue("application", "config/name");
    }
    
    public SemVersion GetProjectGodotVersion()
    {
        string[] features = GetFeatures();
        string version_raw = features[0]; // FIXME: This is a hard code
        return SemVersion.Parse(version_raw, SemVersionStyles.OptionalPatch);
    }

    public string[] GetFeatures()
    {
        return (string[])file.GetValue("application", "config/features");
    }

    public Variant GetValueRaw(string section, string key, Variant @default = default)
    {
        return file.GetValue(section, key, @default);
    }
}