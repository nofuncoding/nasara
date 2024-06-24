using System;
using Godot;
using Nasara.Core.Management.Editor;
using Nasara.UI.Component;

namespace Nasara.UI.View;

public partial class AddingEditorView : Control
{
    // FIXME type cast doesn't work
    [Export] private RequestingContainer _requestingContainer;
    [Export] private ItemList _editorList;
    
    [Signal]
    public delegate void CompletedEventHandler();

    public override async void _Ready()
    {
        // _requestingContainer = GetNode<RequestingContainer>("RequestingContainer");
        
        try
        {
            var manifest = await App.GetGodotManager().GetManifest();
            foreach (var ver in manifest.GetVersionAll())
            {
                _editorList.AddItem(ver.GetVersion().ToString());
            }
            _requestingContainer.Status = RequestingContainer.ViewStatus.Done;
        }
        catch (Exception e)
        {
            _requestingContainer.Status = RequestingContainer.ViewStatus.Error;
        }
        
        //Logger.Log($"{manifest.GetVersionAll()}");
    }
    
}