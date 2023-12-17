using Godot;
using System;

public partial class NotifySystem : Control
{
	[Export]
	VBoxContainer notifyContainer;

	PackedScene ballonRes;

    public override void _Ready()
    {
        ballonRes = GD.Load<PackedScene>("res://component/notify_ballon.tscn");
    }

    public void Notify(NotifyBallon.NotifyType notifyType = NotifyBallon.NotifyType.Info,
		string title="Notify", string description="No description", bool autoHide=true)
	{
		var node = ballonRes.Instantiate<NotifyBallon>();

		node.SetNotifyType(notifyType);
		node.SetTitle(title);
		node.SetDescription(description);
		if (!autoHide)
			node.HideTime = 0f;
		
		notifyContainer.AddChild(node);
	}
}
