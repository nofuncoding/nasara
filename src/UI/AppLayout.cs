using Godot;
using System;
using Nasara.Core;
using Nasara.Core.Management;
using Nasara.UI.Component;

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
    [Export] private Control _loadingPage;
    [ExportSubgroup("Main Page")] 
    [Export] private NavigationBar _navigationBar;
    [Export] private ViewSwitch _viewSwitch;
    [ExportSubgroup("Loading Page")]
    [Export] private ProgressBar _loadingBar;
    
    public override void _Ready()
    {
        App.Initialize(this);
        
        // Load pages
        Logger.Log("Loading UI", "AppLayout");
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/editor_view.tscn"), Tr("Editor"));
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/project_view.tscn"), Tr("Project"));
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/news_view.tscn"), Tr("News"));

        _popupLayer.Hide();
        
        UpdateChecker.CheckUpdate();
        new GodotManager().GetManifest();
    }

    // TODO add some animations here
    public static void ShowPopup(Control popup)
    {
        var instance = App.GetLayout();

        if (popup.IsInsideTree() || instance._popupHolder.GetChildren().Count > 0) return;
        
        instance._popupLayer.Show();
        instance._popupHolder.AddChild(popup);
        popup.TreeExited += instance._popupLayer.Hide;
    }
}