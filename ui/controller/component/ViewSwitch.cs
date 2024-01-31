using Godot;
using System;

namespace Nasara.UI.Component;

public partial class ViewSwitch : Control
{
	// Need to Add Scene Manually
	public Godot.Collections.Dictionary<int, PackedScene> packedView = [];

	[Signal]
	public delegate void ViewSwitchedEventHandler(int viewIndex);

	Godot.Collections.Dictionary<int, Control> availableViews = [];


	public void Init()
	{
		foreach (var (index, packed) in packedView) {
			Control node = (Control)packed.Instantiate();
			AddChild(node);
			//availableViews.Add(view.Key, node.GetPath());
			availableViews.Add(index, node);
		}
		
		foreach (var (index, node) in availableViews) {
			// Control node = GetNode<Control>(view.Value);
			if (index == 0) // 0 is default view index
				node.Visible = true;
			else
				node.Visible = false;
		}
	}

	public void SwitchView(int viewIndex)
	{
		if (!availableViews.ContainsKey(viewIndex))
		{
			GD.PushError("View Index `", viewIndex ,"` Not Exist");
			return;
		}

		foreach (var (index, node) in availableViews) {
			//Control node = GetNode<Control>(view.Value);
			if (index != viewIndex)
				node.Visible = false;
			else
				node.Visible = true;
		}

		EmitSignal(SignalName.ViewSwitched, viewIndex);
	}
}
