using Godot;
using System;
using Nasara.UI.Component;

namespace Nasara.UI;

public partial class NotifySystem : Control
{
	[Export]
	VBoxContainer notifyContainer;

	PackedScene ballonRes;

	public override void _Ready()
	{
		ballonRes = GD.Load<PackedScene>("res://ui/component/notify_ballon.tscn");
	}

	public void Notify(NotificationType type = NotificationType.Info,
		string title="Notify", string description="No description", bool autoHide=true)
	{
		var node = ballonRes.Instantiate<NotifyBallon>();

		node.SetNotifyType(type);
		node.SetTitle(title);
		node.SetDescription(description);
		if (!autoHide)
			node.HideTime = 0f;
		
		notifyContainer.AddChild(node);
	}
}

public enum NotificationType
{
	Info,
	Warn,
	Error,
}
