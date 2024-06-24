using Godot;
using System;
using Nasara.Core;
using Nasara.Core.Management.Editor;
using Nasara.UI.Component;
using Nasara.UI.Component.Notification;

namespace Nasara.UI;

/// <summary>
/// The main layout of this application,
/// will be the first one to run because Godot Engine
/// doesn't support run without default scene.
/// </summary>
public partial class AppLayout : PanelContainer
{

    [Export] private Control _popupLayer;
    [Export] private Control _popupHolder;
    [ExportGroup("Pages")]
    [Export] private Control _mainPage;
    [ExportSubgroup("Main Page")] 
    [Export] private NavigationBar _navigationBar;
    [Export] private ViewSwitch _viewSwitch;
    
    public override async void _Ready()
    {
        App.Initialize(this);
        
        // Load pages
        Logger.Log("Loading UI");
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/editor_view.tscn"), Tr("Editor"));
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/project_view.tscn"), Tr("Project"));
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/news_view.tscn"), Tr("News"));

        // Setup styles
        _popupLayer.Hide();
        
        // Setup notification
        App.NotificationSystem.AddInject(GetNode<InnerNotify>("InnerNotify"));
        App.NotificationSystem.SetInjectType(NotificationInjectType.Software);
        
        await UpdateChecker.CheckUpdate();
    }

    // TODO add some animations here
    public static void ShowPopup(Control popup)
    {
        var instance = App.GetLayout();

        if (popup.IsInsideTree() || instance._popupHolder.GetChildren().Count > 0) return;
        
        instance._popupLayer.Show();
        instance._popupHolder.AddChild(popup);
        popup.TreeExited += instance._popupLayer.Hide;
        /*// half of the window
        instance._popupHolder.Size = instance.GetTree().Root.Size / 2;*/
    }
}