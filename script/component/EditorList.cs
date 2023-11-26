using Godot;
using System;

public partial class EditorList : VBoxContainer
{
	[Export]
	public BaseButton addButton;

	[Export]
	ItemList editorItemList;

	// [{ "index": .., "version": .., "path": .., "channel": .. }, ..]
	Godot.Collections.Array<Godot.Collections.Dictionary> editorItems = new();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RefreshEditors();
	}

	public void RefreshEditors()
	{
		// TODO: Find out the Editors
		GodotManager godotManager = GetNode<GodotManager>("/root/GodotManager");
		Godot.Collections.Array<GodotVersion> godotVersions = godotManager.GetVersions();

		editorItemList.Clear();

		foreach (GodotVersion godot in godotVersions)
		{
			Godot.Collections.Dictionary item = new()
            {
                { "version", godot.Version.ToString() },
                { "path", godot.Path },
                { "channel", godot.Channel.ToString() }
            };

			if (godot.Mono) // Add to item list
				item.Add("index", editorItemList.AddItem($"Godot Mono {godot.Version}"));
			else
				item.Add("index", editorItemList.AddItem($"Godot {godot.Version}"));

			editorItems.Add(item);
		}

		editorItemList.SortItemsByText();
	}
}
