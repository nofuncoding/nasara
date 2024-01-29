using Godot;
using System;

namespace Nasara.Core.Management.Project;

public partial class Project
{
    const string PROJECT_FILE = "project.godot";

    public string ProjectDirPath { get; private set; }
    public string ProjectFilePath { get; private set; }

    public string Name { get; private set; }

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
                throw new Exception("Invalid Project File");
        else if (DirAccess.DirExistsAbsolute(path)) // Is directory
            if (DirHasProjectFile(path))
            {
                ProjectDirPath = path;
                ProjectFilePath = path.PathJoin(PROJECT_FILE);
            }
            else
                throw new Exception("Invalid Project Directory");
        else
            throw new Exception("Invalid Project Path");

        Init();
    }

    void Init()
    {
        // Load the project file
        projectFile = new ProjectFile(ProjectFilePath);
        Name = projectFile.GetProjectName();
    }

    static bool DirHasProjectFile(string dirPath)
    {
        var dir = DirAccess.Open(dirPath);
        if (dir is null)
            GD.PushError(DirAccess.GetOpenError());
        
        // Get all files in *the root directory*
        var all_files = dir.GetFiles();

        foreach (var file in all_files)
            if (file == PROJECT_FILE)
                return true;
        return false;
    }
}