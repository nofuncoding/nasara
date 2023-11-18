using Godot;
using System;
using System.Collections.Generic;

public partial class ViewSwitch : Control
{
	// Need to Add Scene Manually
	public Godot.Collections.Dictionary<int, PackedScene> packedView = new();

	[Signal]
	public delegate void ViewSwitchedEventHandler(int viewIndex);

	Godot.Collections.Dictionary<int, Control> availableViews = new();


    public void Init()
    {
        foreach (KeyValuePair<int, PackedScene> view in packedView) {
			Control node = (Control)view.Value.Instantiate();
			AddChild(node);
			//availableViews.Add(view.Key, node.GetPath());
			availableViews.Add(view.Key, node);
		}
		
		foreach (KeyValuePair<int, Control> view in availableViews) {
			// Control node = GetNode<Control>(view.Value);
			if (view.Key == 0) // 0 is default view index
				view.Value.Visible = true;
			else
				view.Value.Visible = false;
		}
    }

	public void SwitchView(int viewIndex)
	{
		if (!availableViews.ContainsKey(viewIndex))
		{
			GD.PushError("View Index `", viewIndex ,"` Not Exist");
			return;
		}

		foreach (KeyValuePair<int, Control> view in availableViews) {
			//Control node = GetNode<Control>(view.Value);
			if (view.Key != viewIndex)
				view.Value.Visible = false;
			else
				view.Value.Visible = true;
		}

		EmitSignal(SignalName.ViewSwitched, viewIndex);
	}
}
