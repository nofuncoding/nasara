using Godot;
using System;
using Nasara.UI.Component;

namespace Nasara.UI.View;

public partial class ProjectView : Control
{
	[Export]
	ProjectList projectList;

	[Export]
	string addProjectViewPath;

	public override void _Ready()
	{
		projectList.addButton.Pressed += AddProject;
	}

	void AddProject()
	{
		PackedScene res = GD.Load<PackedScene>(addProjectViewPath);
		AddProjectView addProjectView = res.Instantiate<AddProjectView>();
		AddChild(addProjectView);
		addProjectView.Completed += () => {
			addProjectView.QueueFree();
			GetNode<VBoxContainer>("VBoxContainer").Visible = true;
			projectList.RefreshProjects();
		};

		GetNode<VBoxContainer>("VBoxContainer").Visible = false;
	}
}
