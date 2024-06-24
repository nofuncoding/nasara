using Godot;
using System;
using Nasara.Core;

namespace Nasara.UI.View;

public partial class SettingView : Control
{
    [Export]
	private BaseButton _checkUpdateButton;
    
	public override void _Ready()
	{
        _checkUpdateButton.Pressed += CheckUpdate;
	}
    
    private async void CheckUpdate()
    {
        _checkUpdateButton.Disabled = true;
        await UpdateChecker.CheckUpdate();
        _checkUpdateButton.Disabled = false;
    }
}
