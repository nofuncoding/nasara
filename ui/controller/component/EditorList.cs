using Godot;
using System;
using Editor = Nasara.Core.Management.Editor;
using static Nasara.Core.Management.Editor.GodotVersion;

namespace Nasara.UI.Component;

public partial class EditorList : Control
{
	[Export]
	Control emptyDisplay;
	[Export]
	ScrollContainer scroller;
	[Export]
	VBoxContainer editorListContainer;

	Editor.Manager _manager;
	PackedScene _packedEditorHolder;

	public override void _Ready()
	{
		_manager = App.GetEditorManager();
		_packedEditorHolder = GD.Load<PackedScene>("res://ui/component/news_holder.tscn");

		// _manager.OnEditorListChanged += OnEditorListChanged;
		RefreshEditorList();
	}

	public void RefreshEditorList()
	{
		// FIXME: not displaying
		var versions = Editor.Version.GetVersions();

		// Remove the old list
		foreach (var c in editorListContainer.GetChildren())
			c.QueueFree();

		if (versions.Count == 0)
		{
			scroller.Hide();
			emptyDisplay.Show();
			return;
		}
		else
		{
			scroller.Show();
			emptyDisplay.Hide();
		}

		foreach (var version in versions)
		{
			var holder = _packedEditorHolder.Instantiate() as EditorHolder;
			holder.Version ??= version;
			editorListContainer.AddChild(holder);
		}
	}
}
