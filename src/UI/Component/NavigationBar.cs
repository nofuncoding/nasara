using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nasara.UI.Component;

public partial class NavigationBar : Control
{
    [Export] private ViewSwitch _viewSwitch;
    [Export] private Label _versionLabel;
    [Export] private MenuButton _menuButton;
    [Export] private VerticalButtonContainer _buttonContainer;

    private Dictionary<int, BaseButton> _buttonDictionary = [];

    public override void _Ready()
    {
        _buttonContainer.ButtonGroup.Pressed += ChangeNav;

        PopupMenu menu = _menuButton.GetPopup();
        menu.AddItem(Tr("About")+"...", 0);
        menu.IdPressed += MenuIdPressed;
        menu.Transparent = true;
        menu.TransparentBg = true;

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

        App.Log($"Loaded {packedScene.ResourcePath} as index {index}", "NavigationBar");
        return index;
    }

    private void MenuIdPressed(long id)
    {
        switch (id)
        {
            case 0: // about
                var appInfo = GD.Load<PackedScene>("res://ui/component/popup/app_info_popup.tscn").Instantiate<Window>();
                AddChild(appInfo);
                appInfo.PopupCentered();
                break;
        }
    }

    private void ChangeNav(BaseButton button)
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