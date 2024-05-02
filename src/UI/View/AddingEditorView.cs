using System;
using Godot;

namespace Nasara.UI.View;

public partial class AddingEditorView : Control
{
    [Signal]
    public delegate void CompletedEventHandler();

    public override void _Ready()
    {
        
    }
}