using Godot;
using System;
using Project = Nasara.Core.Management.Project;

namespace Nasara.UI.Component;

// NOTE: This is a duplicate of EditorList
// if something changed inside EditorList, this should be changed too

public partial class ProjectList : VBoxContainer
{
    [Export]
	ItemList projectItemList;
	[Export]
	Label tipNotFound;

	[ExportCategory("Button")]
	[Export]
	public BaseButton addButton;
	[Export]
	public BaseButton launchButton;

    // [{"name": ..., "path": ..., "godot_version": ...}, ..]
    Godot.Collections.Array<Godot.Collections.Dictionary> _projectItems = [];
    Project.Manager _projectManager;

    public override void _Ready()
    {
        _projectManager = App.GetProjectManager();

        launchButton.Disabled = true;
        launchButton.Pressed += LaunchProject;

        projectItemList.ItemActivated += LaunchProject;
        projectItemList.EmptyClicked += (Vector2 pos, long mouseButton) =>
		{
			projectItemList.DeselectAll();
			launchButton.Disabled = true;
		};
		projectItemList.ItemSelected += (long index) =>
		{
			launchButton.Disabled = false;
		};

        projectItemList.ItemClicked += (long index, Vector2 at_position, long mouse_button_index) =>
		{
			if (mouse_button_index == 2) // Right click
			{
				PopupMenu menu = new()
				{
					Position = DisplayServer.MouseGetPosition(),
					Transparent = true,
					TransparentBg = true,
				};
				AddChild(menu);
				menu.AddItem(Tr("Launch"), 0);
				menu.AddItem(Tr("Delete"), 1);
                menu.SetItemDisabled(1, true); // TODO: Deleting project is not implemented yet
				menu.IdPressed += (long id) => { PopupMenuHandle(id, index); menu.QueueFree(); };
				menu.Popup();
			}
		};
    
        RefreshProjects();
    }

    /*public override void _Process(double delta)
    {
		GD.Print($"items: [{projectItemList.IsAnythingSelected()}] {projectItemList.GetSelectedItems().Stringify()}");
    }*/

    Godot.Collections.Dictionary GetCurrentProject(int index)
	{
		string selectedName = projectItemList.GetItemText(index);
		foreach (var project in _projectItems)
			if ((string)project["name"] == selectedName &&
                // get the metadata binded to the item
                (string)projectItemList.GetItemMetadata(index) == (string)project["path"])
				return project;
		
		return null;
	}

    public void RefreshProjects()
    {
        Godot.Collections.Array<Project.Project> projects = _projectManager.GetProjects();

        projectItemList.Clear();
        _projectItems.Clear();

        if (projects.Count == 0)
        {
            launchButton.Disabled = true;
            tipNotFound.Visible = true;
			return;
        }

        foreach (Project.Project project in projects)
        {
            Godot.Collections.Dictionary item = new()
            {
                {"name", project.Name},
                {"path", project.ProjectFilePath},
                {"instance", project},
            };

            var i = projectItemList.AddItem((string)item["name"]);
            projectItemList.SetItemMetadata(i, item["path"]);
			projectItemList.SetItemTooltip(i, (string)item["path"]);

            _projectItems.Add(item);
        }

        projectItemList.SortItemsByText();
        tipNotFound.Visible = false;
    }

    void PopupMenuHandle(long index, long on_item)
	{
		switch (index)
		{
			case 0: // Launch
				LaunchProject(on_item); break;
			case 1: // Delete
				/*ConfirmationDialog dialog = new()
				{
					Title = Tr("Delete Editor"),
					DialogText = Tr("Are you sure you want to delete this editor?"),
					Transparent = true,
					TransparentBg = true,
					OkButtonText = Tr("Yes")
				};
				dialog.Confirmed += () => {
					DeleteEditor((Editor.GodotVersion)GetCurrentEditor((int)on_item)["version"]);
					dialog.QueueFree();
				};
				dialog.Canceled += () => { dialog.QueueFree(); };

				AddChild(dialog);
				dialog.PopupCentered();*/
				
				break;
		}
    }

    void LaunchProject() 
    {
		// FIXME: returning a empty array (issue #27)
        /*
		int[] items = projectItemList.GetSelectedItems();

		// only allows single selection
		if (items.Length > 1 || items.Length == 0)
			return;
		

		int index = items[0];
		*/

		// Simple fix
		for (int i = 0; i < projectItemList.ItemCount; i++)
		{
			if (projectItemList.IsSelected(i))
			{
				LaunchProject(i);
				return;
			}
		}
		
    }

    void LaunchProject(long index)
    {
        /*_projectManager.LaunchProject(*/
            GD.Print((Project.Project)GetCurrentProject((int)index)["instance"]);
            /*Core.Management.Editor.Version.GetVersions()[0]
        );*/
    }
}
