using Godot;
using System;
using Editor = Nasara.Core.Management.Editor;
using static Nasara.Core.Management.Editor.GodotVersion;

namespace Nasara.UI.Component;

public partial class EditorList : VBoxContainer
{
	[Export]
	ItemList editorItemList;
	[Export]
	Label tipNotFound;

	[ExportCategory("Button")]
	[Export]
	public BaseButton addButton;
	[Export]
	public BaseButton launchButton;

	// [{"version": .., "path": .., "channel": .. }, ..]
	Godot.Collections.Array<Godot.Collections.Dictionary> editorItems = [];
	Editor.Manager _editorManager;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_editorManager = App.GetEditorManager();
		
		launchButton.Disabled = true;
		launchButton.Pressed += LaunchEditor;

		editorItemList.ItemActivated += LaunchEditor;
		editorItemList.EmptyClicked += (Vector2 pos, long mouseButton) =>
		{
			editorItemList.DeselectAll();
			launchButton.Disabled = true;
		};
		editorItemList.ItemSelected += (long index) =>
		{
			launchButton.Disabled = false;
		};

		editorItemList.ItemClicked += (long index, Vector2 at_position, long mouse_button_index) =>
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
				menu.IdPressed += (long id) => { PopupMenuHandle(id, index); menu.QueueFree(); };
				menu.Popup();
			}
		};
		
		RefreshEditors();
	}

	Godot.Collections.Dictionary GetCurrentEditor(int index)
	{
		string selectedName = editorItemList.GetItemText(index);
		foreach (Godot.Collections.Dictionary editor in editorItems)
			if ((string)editor["name"] == selectedName) // TODO: Use another way
				return editor;
		
		return null;
	}

	void PopupMenuHandle(long index, long on_item)
	{
		switch (index)
		{
			case 0: // Launch
				LaunchEditor(on_item); break;
			case 1: // Delete
				ConfirmationDialog dialog = new()
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
				dialog.PopupCentered();
				
				break;
		}
	}

	public void RefreshEditors()
	{
		// TODO: Scan for the Editors
		Godot.Collections.Array<Editor.GodotVersion> godotVersions = Editor.Version.GetVersions();

		editorItemList.Clear();
		editorItems.Clear();

		if (godotVersions.Count == 0)
		{
			launchButton.Disabled = true;
			tipNotFound.Visible = true;
			return;
		}

		foreach (Editor.GodotVersion godot in godotVersions)
		{
			string version_name = $"Godot {godot.Version}";
			if (godot.Mono)
				version_name = $"Godot Mono {godot.Version}";


			Godot.Collections.Dictionary item = new()
			{
				{ "name", version_name },
				{ "version", godot },
			};
			if (godot.Status != VersionStatus.OK)
			{
				switch (godot.Status)
				{
					case VersionStatus.NotFound:
						var i = editorItemList.AddItem(version_name);
						editorItemList.SetItemDisabled(i, true);
						editorItemList.SetItemTooltip(i, "Executable Not Found");
						break;
				}
			}
			else
			{
				editorItemList.AddItem(version_name);
			}

			editorItems.Add(item);
		}

		editorItemList.SortItemsByText();
		tipNotFound.Visible = false;
	}

	void LaunchEditor()
	{
		// returning a empty array (issue #27)
		// if Godot version is 4.3-dev3

		
		int[] items = editorItemList.GetSelectedItems();

		// only allows single selection
		if (items.Length > 1)
			return;
		
		int index = items[0];
		
		LaunchEditor(index);
		
		// Simple fix for above
		/*
		for (int i = 0; i < editorItemList.ItemCount; i++)
		{
			if (editorItemList.IsSelected(i))
			{
				LaunchEditor(i);
				return;
			}
		}
		*/
	}

	void LaunchEditor(long index)
	{
		_editorManager.Launch((Editor.GodotVersion)GetCurrentEditor((int)index)["version"]);
	}

	void DeleteEditor(Editor.GodotVersion version)
	{
		string path = version.Path;
		OS.MoveToTrash(ProjectSettings.GlobalizePath(path)); // TODO: Use DirAccess to delete
		_editorManager.Version.RemoveVersion(version);
		RefreshEditors();
	}
}
