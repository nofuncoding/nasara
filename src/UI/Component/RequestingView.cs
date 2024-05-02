using Godot;
using System;

namespace Nasara.UI.Component;

public partial class RequestingContainer : PageSwitch
{
	private ViewStatus _viewStatus = ViewStatus.Idle;
	public ViewStatus Status
	{
		get => _viewStatus;
		set => SetStatus(value);
	}

	public override void _Ready()
	{
		foreach (var n in GetChildren())
		{
			if (n is Control c)
				Pages[Pages.Length] = c;
		}
	}

	private void SetStatus(ViewStatus viewStatus)
	{
		if (viewStatus == _viewStatus) return;

		_viewStatus = viewStatus;

		CurrentPageIndex = _viewStatus switch
		{
			ViewStatus.Idle => -1,
			ViewStatus.Requesting => 0,
			ViewStatus.Error => 1,
			ViewStatus.Done => 2,
			_ => -1,
		};
	}

	public enum ViewStatus
	{
		Idle,
		Requesting,
		Error,
		Done
	}
}
