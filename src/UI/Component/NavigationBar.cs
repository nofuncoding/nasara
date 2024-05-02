using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nasara.UI.Component;

public partial class NavigationBar : Control
{
    [Export] private ViewSwitch _viewSwitch;
    [Export] private Label _versionLabel;
    [Export] private Button _settingButton;
    [Export] private VerticalButtonContainer _buttonContainer;

    private Dictionary<int, BaseButton> _buttonDictionary = [];

    public override void _Ready()
    {
        _buttonContainer.ButtonGroup.Pressed += Navigate;
        _settingButton.Pressed += () =>
        {
            var scene = GD.Load<PackedScene>("res://ui/view/setting_view.tscn").Instantiate<Control>();
            AppLayout.ShowPopup(scene);
        };

        if (_versionLabel is null) return;
        
        if (OS.IsDebugBuild())
        {
            _versionLabel.Visible = true;
            _versionLabel.Text = $"v{App.GetVersion()}";
        }
        else
            _versionLabel.Visible = false;
    }

    public int RegisterView(PackedScene packedScene, string displayName)
    {
        var index = _viewSwitch.AddView(packedScene); // add scene to view switch
        var viewButton = _buttonContainer.AddButton(displayName); // make it display on nav
        
        _buttonDictionary.Add(index, viewButton);

        Logger.Log($"Loaded {packedScene.ResourcePath} as index {index}", "NavigationBar");
        return index;
    }

    private void Navigate(BaseButton button)
    {
        // Find out the given button
        foreach (var view in _buttonDictionary.Where(view => view.Value == button))
        {
            if (view.Key == _viewSwitch.CurrentPageIndex)
                return;

            // switch to the view
            _viewSwitch.CurrentPageIndex = view.Key;
        }
    }
}