using Godot;
using System;
using Project = Nasara.Core.Management.Project;

namespace Nasara.UI.View;

public partial class ProjectView : Control
{
	[Export]
	LineEdit lineEdit;
	[Export]
	RichTextLabel rtl;

	Project.Manager manager;

	public override void _Ready()
	{
		manager = new();
		AddChild(manager);

		lineEdit.TextChanged += (string text) => {
			try {
				Project.Project project = new(text);
				rtl.Text = $"Name: {project.Name}\nVersion: {project.UsingGodotVersion}";
			} catch (Exception) {
				rtl.Text = "Failed";
			}
		};
	}
}
