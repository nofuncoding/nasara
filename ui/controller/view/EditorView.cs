using Godot;
using System;
using Nasara.UI.Component;

namespace Nasara.UI.View;

public partial class EditorView : Control
{

	[Export]
	EditorList editorList;

	[Export]
	string addEditorViewPath;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		editorList.addButton.Pressed += AddEditor;
	}

	void AddEditor()
	{
		PackedScene res = GD.Load<PackedScene>(addEditorViewPath);
		AddEditorView addEditorView = res.Instantiate<AddEditorView>();
		AddChild(addEditorView);
		addEditorView.Completed += () => {
			addEditorView.QueueFree();
			GetNode<VBoxContainer>("VBoxContainer").Visible = true;
			editorList.RefreshEditors();
		};

		GetNode<VBoxContainer>("VBoxContainer").Visible = false;
	}
}
