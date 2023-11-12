using Godot;
using System;

public partial class EditorView : Control
{

	// TODO: Use `Tree` instead of `ItemList`

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RefreshEditors();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void RefreshEditors()
	{
		// TODO: Find out the Editors
	}
}
