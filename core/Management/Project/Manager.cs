using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace Nasara.Core.Management.Project;

// TODO: refactor to static
public partial class Manager : Node
{
    Array<Project> projects;

    public Manager()
    {
        Refresh();
    }

    public Array<Project> GetProjects() => projects;

    public bool ProjectExists(string path)
    {
        foreach (var i in projects)
        {
            if (i.ProjectFilePath == path || i.ProjectDirPath == path)
                return true;
        }
        return false;
    }

    public bool ProjectExists(Project project)
    {
        foreach (var i in projects)
        {
            if (i.ProjectFilePath == project.ProjectFilePath)
                return true;
        }
        return false;
    }

    public void Refresh()
    {
        projects = ProjectList.Read();
        var editors_project = GetLocalProjectsFromEditors();
        foreach (var i in editors_project)
        {
            // may have something wrong
            if (projects.Contains(i))
                continue;
            else
                projects.Add(i);
        }
    }

    public void Add(Project project)
    {
        ProjectList.Add(project);
        Refresh();
    }

    public void Remove(Project project)
    {
        ProjectList.Remove(project);
        Refresh();
    }

    /// <summary>
    /// Get local projects from the editors saved projects.
    ///
    /// It usually saves the project list in the user folder.
    /// The file path is `user://projects.cfg` (note: the path is for the godot editor)
    /// </summary>
    public static Project[] GetLocalProjectsFromEditors()
    {
        ConfigFile config = new();
        var file_path = GetLocalProjectsFilePath();
        if (file_path is null)
            return [];

        var err = config.Load(file_path);
        if (err != Error.Ok)
        {
            GD.PushError($"Failed to load projects.cfg: {err}");
            return [];
        }

        string[] paths = config.GetSections();
        System.Collections.Generic.List<Project> p = [];
        foreach (var path in paths)
        {
            try
            {
                Project project = new(path);
                p.Add(project);
            }
            catch (Exception e)
            {
                GD.PushError($"Failed to load project \"{path}\": {e.Message}");
            }
        }

        // TODO: save to project list

        return [.. p];
    }

    /// <summary>
    /// Launches a project using the given godot version.
    /// </summary>
    /// <param name="project">The project to launch.</param>
    /// <param name="godot">The godot version to launch the project with.</param>
    /// <returns></returns>
    public Error LaunchProject(Project project, Editor.GodotVersion godot)
    {
        Editor.Launcher launcher = new(godot);
        AddChild(launcher);
        return launcher.Launch(project.ProjectDirPath);
    }

    static string GetLocalProjectsFilePath()
    {
        const string LOCAL_EDITORS_FILE = "projects.cfg";

        const string LINUX_GODOT_DATA_DIR = "~/.local/share/godot/";
        const string FLATPAK_GODOT_DATA_DIR = "~/.var/app/org.godotengine.godot/";
        const string MACOS_GODOT_DATA_DIR = "~/Library/Application Support/Godot/";

        string file_path;

        if (OperatingSystem.IsWindows())
        {
            string appdata = System.Environment.GetEnvironmentVariable("APPDATA") ?? "C:/Users/Administrator/AppData/Roaming/";
            if (appdata is null)
                return null;
            file_path = appdata.PathJoin("/Godot/").PathJoin(LOCAL_EDITORS_FILE);
        }
        else if (OperatingSystem.IsLinux())
        {
            if (DirAccess.DirExistsAbsolute(FLATPAK_GODOT_DATA_DIR))
                file_path = FLATPAK_GODOT_DATA_DIR.PathJoin(LOCAL_EDITORS_FILE);
            else
                file_path = LINUX_GODOT_DATA_DIR.PathJoin(LOCAL_EDITORS_FILE);
        }
        else if (OperatingSystem.IsMacOS())
        {
            file_path = MACOS_GODOT_DATA_DIR.PathJoin(LOCAL_EDITORS_FILE);
        }
        else
        {
            throw new SystemException("Operating system not supported");
        }

        if (FileAccess.FileExists(file_path))
            return file_path;
        else
            return "";
    }

    // TODO
}

static class ProjectList
{
    /* Example:
    [
        {
            "name": "xxx",
            "path": "/path/to/project/dir"
        },
        ...
    ]*/

    public const string PROJECT_LIST = "user://projects.json";

    public static Array<Project> Read()
    {
        var raw = ReadRaw();
        if (raw.Count <= 0)
            return [];
        else
            return ParseFromRaw(raw);
    }

    public static void Add(Project project)
    {
        var projects = Read();

        foreach (var p in projects)
            if (p.ProjectFilePath == project.ProjectFilePath)
            {
                if (p.Name != project.Name)
                    Rename(p.Name, project);
                else
                    return;
            }
        
        projects.Add(project);

        Write(projects);
    }

    public static void Add(string path)
    {
        Project project = new(path);
        Add(project);
    }
    
    public static void Remove(Project project)
    {
        var projects = Read();
        var parsed = projects;
        foreach (var p in projects)
        {
            if (p.ProjectFilePath == project.ProjectFilePath)
            {
                parsed.Remove(p);
                break;
            }
        }
    
        Write(parsed);
    }

    /// <summary>
    /// This function is used to rename a project, the project's
    /// path must be given
    /// </summary>
    public static void Rename(string prev_name, Project new_project)
    {
        var raw = ReadRaw();
        for (int i = 0; i < raw.Count; i++)
        {
            if (raw[i]["name"] == prev_name && raw[i]["path"] == new_project.ProjectFilePath)
            {
                raw[i]["name"] = new_project.Name;
                break;
            }
        }

        Write(raw);
    }

    static Array<Dictionary<string, string>> ReadRaw()
    {
        if (!FileAccess.FileExists(PROJECT_LIST))
        {
            FileAccess.Open(PROJECT_LIST, FileAccess.ModeFlags.Write); // Create it
            return [];
        }

        using var file = FileAccess.Open(PROJECT_LIST, FileAccess.ModeFlags.Read);
        if (file is null)
            GD.PushError($"Failed to read {PROJECT_LIST}: {FileAccess.GetOpenError()}");

        string fileText = file.GetAsText();
        if (fileText.Length <= 0) // file is empty
            return [];

        Json fileJson = new();
        if (fileJson.Parse(fileText) != Error.Ok)
        {
            GD.PushError($"Failed to parse {PROJECT_LIST} (line {fileJson.GetErrorLine()}): {fileJson.GetErrorMessage()}");
            return [];
        }
        
        var data = (Array<Dictionary<string, string>>)fileJson.Data;
        return data;
    }

    static void Write(Array<Dictionary<string, string>> data)
    {
        GD.Print("Writing to " + PROJECT_LIST);

        using var file = FileAccess.Open(PROJECT_LIST, FileAccess.ModeFlags.Write);
        if (file is not null)
            file.StoreString(Json.Stringify(data));
        else
            GD.PushError($"Failed to write to {PROJECT_LIST}: {FileAccess.GetOpenError()}");
    }

    static void Write(Array<Project> projects)
    {
        GD.Print("Writing to " + PROJECT_LIST);

        using var file = FileAccess.Open(PROJECT_LIST, FileAccess.ModeFlags.Write);
        if (file is not null)
            file.StoreString(Json.Stringify(ParseToRaw(projects)));
        else
            GD.PushError($"Failed to write to {PROJECT_LIST}: {FileAccess.GetOpenError()}");
    }

    /// <summary>
    /// Parse Project objects into raw data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static Array<Dictionary<string, string>> ParseToRaw(Array<Project> data)
    {
        Array<Dictionary<string, string>> parsed_list = [];

        foreach (Project project in data)
        {
            Dictionary<string, string> parsed = new()
            {
                { "name", project.Name },
                { "path", project.ProjectFilePath }
            };

            parsed_list.Add(parsed);
        }

        return parsed_list;
    }

    /// <summary>
    /// Parse raw data into Project objects
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static Array<Project> ParseFromRaw(Array<Dictionary<string, string>> data)
    {
        Array<Project> parsed = [];

        foreach (var p in data)
        {
            string path = p["path"];

            try
            {
                Project project = new(path);

                if (project.Name != p["name"])
                    Rename(p["name"], project);

                parsed.Add(project);
            }
            catch (Exception e)
            {
                GD.PushError($"{e.Message}: {path}");
            }
        }

        // If there are error projects, ignore them
        // and write the rest
        // NOTE: may not fit any more in the future
        if (parsed.Count < data.Count)
            Write(parsed);
        
        return parsed;
    }
}