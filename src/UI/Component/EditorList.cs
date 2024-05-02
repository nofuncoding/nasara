using System;
using Godot;
using Nasara.UI.View;

namespace Nasara.UI.Component;

public partial class EditorList : Control
{
    [Export] private Control _emptyTip;
    [Export] private ScrollContainer _scroller;
    [Export] private VBoxContainer _editorContainer;
    [ExportGroup("Button")]
    [Export] private Button _addingButton;

    private PackedScene _packedEditorHolder;

    [Signal] public delegate void AddingEditorEventHandler();
    
    public override void _Ready()
    {
        _addingButton.Pressed += () => EmitSignal(SignalName.AddingEditor);
        _packedEditorHolder = GD.Load<PackedScene>("res://ui/component/editor_holder.tscn");
        
        RefreshEditors();
    }

    public void RefreshEditors()
    {
        
    }
}