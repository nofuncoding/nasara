using Godot;
using Nasara.UI.Component;

namespace Nasara.UI.View;

public partial class EditorView : Control
{
    [Export] private EditorList _editorList;
    [Export(PropertyHint.File)] private string _addingEditorViewFilePath;

    public override void _Ready()
    {
        _editorList.AddingEditor += AddEditor;
    }

    private void AddEditor()
    {
        var packed = GD.Load<PackedScene>(_addingEditorViewFilePath);
        var addingEditorView = packed.Instantiate<AddingEditorView>();
        AppLayout.ShowPopup(addingEditorView);

        addingEditorView.Completed += () =>
        {
            addingEditorView.QueueFree();
            GetNode<VBoxContainer>("VBoxContainer").Visible = true;
            _editorList.RefreshEditors();
        };
        
        GetNode<VBoxContainer>("VBoxContainer").Visible = false;
    }
}