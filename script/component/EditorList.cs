using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class EditorList : VBoxContainer
{
	[Export]
	ItemList editorItemList;

	[ExportCategory("Button")]
	[Export]
	public BaseButton addButton;
	[Export]
	public BaseButton launchButton;

	// [{ "index": .., "version": .., "path": .., "channel": .. }, ..]
	Godot.Collections.Array<Godot.Collections.Dictionary> editorItems = new();
	GodotManager.Manager godotManager;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		godotManager = GetNode<GodotManager.Manager>("/root/GodotManager");
		
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
		RefreshEditors();
	}

	public void RefreshEditors()
	{
		// TODO: Find out the Editors
		Godot.Collections.Array<GodotVersion> godotVersions = godotManager.Version().GetVersions();

		editorItemList.Clear();

		foreach (GodotVersion godot in godotVersions)
		{
			string version_name = $"Godot {godot.Version}";
			if (godot.Mono)
				version_name = $"Godot Mono {godot.Version}";


			Godot.Collections.Dictionary item = new()
            {
				{ "name", version_name },
                { "version", godot },
            };
			if (godot.Status != GodotVersion.VersionStatus.OK)
			{
				switch (godot.Status)
				{
					case GodotVersion.VersionStatus.NotFound:
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
	}

	void LaunchEditor()
	{
		int[] items = editorItemList.GetSelectedItems();
		if (items.Length>1)
			return; // Do not accept multiselect
		
		int index = items[0];
		
		LaunchEditor(index);
	}

	void LaunchEditor(long index)
	{
		string selectedName = editorItemList.GetItemText((int)index);
		foreach (Godot.Collections.Dictionary editor in editorItems)
		{
			if ((string)editor["name"] == selectedName)
			{
				godotManager.Launch((GodotVersion)editor["version"]);
				return;
			}
		}
	}
}
