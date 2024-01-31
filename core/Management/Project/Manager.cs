using Godot;
using Godot.Collections;
using System;

namespace Nasara.Core.Management.Project;

public partial class Manager : Node
{
    Array<Project> versions;

    public override void _Ready()
    {
        versions = ProjectList.Read();
    }

    public void Add(Project project)
    {
        ProjectList.Add(project);
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
            return ParseFrom(raw);
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
        foreach (var p in projects)
        {
            if (p.ProjectFilePath == project.ProjectFilePath)
            {
                projects.Remove(p);
                break;
            }
        }
    
        Write(projects);
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
            file.StoreString(Json.Stringify(ParseTo(projects)));
        else
            GD.PushError($"Failed to write to {PROJECT_LIST}: {FileAccess.GetOpenError()}");
    }

    static Array<Dictionary<string, string>> ParseTo(Array<Project> data)
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

    static Array<Project> ParseFrom(Array<Dictionary<string, string>> data)
    {
        Array<Project> parsed = [];

        foreach (var p in data)
        {
            string path = p["path"];

            try {
                Project project = new(path);

                if (project.Name != p["name"])
                    Rename(p["name"], project);

                parsed.Add(project);
            }
            catch (Exception)
            {
                GD.PushError($"Invalid Project Path: {path}");
            }
        }
        
        return parsed;
    }
}