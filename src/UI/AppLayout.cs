using Godot;
using System;
using Nasara.UI.Component;

namespace Nasara.UI;

/// <summary>
/// The main layout of this application,
/// will be the first one to run because Godot Engine
/// doesn't support run without default scene.
/// </summary>
public partial class AppLayout : PanelContainer
{
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
        App.Initialize();
        
        // Load pages
        App.Log("Loading UI", "AppLayout");
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/editor_view.tscn"), Tr("Editor"));
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/project_view.tscn"), Tr("Project"));
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/news_view.tscn"), Tr("News"));
        _navigationBar.RegisterView(GD.Load<PackedScene>("res://ui/view/setting_view.tscn"), Tr("Setting"));
    }
}