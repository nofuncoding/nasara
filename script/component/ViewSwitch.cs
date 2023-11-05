using Godot;
using System;
using System.Collections.Generic;

public partial class ViewSwitch : Control
{
	[Export]
	Godot.Collections.Dictionary<StringName, string> viewPaths = new Godot.Collections.Dictionary<StringName, string>();

	[Export]
	StringName defaultView;

	Godot.Collections.Dictionary<StringName, NodePath> availableViews = new Godot.Collections.Dictionary<StringName, NodePath>();

    public override void _Ready()
    {
        foreach (KeyValuePair<StringName, string> view in viewPaths) {
			Node node = GD.Load<PackedScene>(view.Value).Instantiate();
			AddChild(node);
			availableViews.Add(view.Key, node.GetPath());
		}

		foreach (KeyValuePair<StringName, NodePath> view in availableViews) {
			Control node = GetNode<Control>(view.Value);
			if (view.Key != defaultView)
				node.Visible = false;
			else
				node.Visible = true;
		}
    }

	public void SwitchView(StringName viewName) {
		if (!availableViews.ContainsKey(viewName))
		{
			GD.PushError("View `", viewName ,"` Not Exist");
			return;
		}

		foreach (KeyValuePair<StringName, NodePath> view in availableViews) {
			Control node = GetNode<Control>(view.Value);
			if (view.Key != viewName)
				node.Visible = false;
			else
				node.Visible = true;
		}
	}
}
