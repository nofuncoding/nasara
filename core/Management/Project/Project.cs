using Godot;
using Semver;
using System;

namespace Nasara.Core.Management.Project;

public partial class Project : RefCounted
{
    const string PROJECT_FILE = "project.godot";

    public string ProjectDirPath { get; private set; }
    public string ProjectFilePath { get; private set; }

    public string Name { get; private set; }
    public SemVersion UsingGodotVersion { get; private set; }

    ProjectFile projectFile;

    public Project(string path)
    {
        path = ProjectSettings.GlobalizePath(path);
        if (FileAccess.FileExists(path)) // Is file
            if (path.GetFile() == PROJECT_FILE)
            {
                ProjectDirPath = path.GetBaseDir();
                ProjectFilePath = path;
            }
            else
                throw new System.IO.IOException("Invalid Project File");
        else if (DirAccess.DirExistsAbsolute(path)) // Is directory
            if (DirHasProjectFile(path))
            {
                ProjectDirPath = path;
                ProjectFilePath = path.PathJoin(PROJECT_FILE);
            }
            else
                throw new System.IO.IOException("Invalid Project Directory");
        else
            throw new System.IO.IOException("Invalid Project Path");

        // Load the project file
        projectFile = new ProjectFile(ProjectFilePath);
        Name = projectFile.GetProjectName();
        UsingGodotVersion = projectFile.GetProjectGodotVersion();
    }

    static bool DirHasProjectFile(string dirPath)
    {
        using var dir = DirAccess.Open(dirPath);
        if (dir is null)
        {
            GD.PushError(DirAccess.GetOpenError());
            return false;
        }
        
        // Get all files in *the root directory*
        var all_files = dir.GetFiles();

        foreach (var file in all_files)
            if (file == PROJECT_FILE)
                return true;
        return false;
    }
}