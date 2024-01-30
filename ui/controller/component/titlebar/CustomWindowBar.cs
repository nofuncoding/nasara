using Godot;
using System;

namespace Nasara.UI.Component.Titlebar;

// NOTE: top level of container grow direction needs to be Right and Bottom

public partial class CustomWindowBar : PanelContainer
{
	DragComponent dragComponent;
	StateComponent stateComponent;
	ResizeComponent resizeComponent;

	[Export]
	Button MinWindowButton;
	[Export]
	Button MaxWindowButton;
	[Export]
	Button CloseWindowButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		dragComponent = GetNode<DragComponent>("DragComponent");
		stateComponent = GetNode<StateComponent>("StateComponent");
		resizeComponent = GetNode<ResizeComponent>("ResizeComponent");
	
		MinWindowButton.Pressed += stateComponent.SetMinimized;
		MaxWindowButton.Pressed += stateComponent.ToggleMaximized;
		CloseWindowButton.Pressed += stateComponent.Quit;

		dragComponent.DragStarted += () => {
			resizeComponent.Activate = false;
		};
		dragComponent.DragStopped += () => {
			resizeComponent.Activate = true;
		};
	}

    public override void _Process(double delta)
    {
        var isResize = resizeComponent.IsWaitingResize() || resizeComponent.IsResizing();
		MinWindowButton.Disabled = isResize;
		MaxWindowButton.Disabled = isResize;
		CloseWindowButton.Disabled = isResize;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
		{
			if (mb.IsPressed() && mb.ButtonIndex == MouseButton.Left)
			{
				if (resizeComponent.IsWaitingResize())
				{
					resizeComponent.StartResize();
					GetViewport().SetInputAsHandled();
				}
			}
			else if (!mb.IsPressed() && mb.ButtonIndex == MouseButton.Left)
			{
				if (resizeComponent.IsResizing())
				{
					resizeComponent.StopResize();
					GetViewport().SetInputAsHandled();
				}
			}
		}

		if (@event is InputEventMouseMotion m)
		{
			if (m.ButtonMask == MouseButtonMask.Left)
			{
				if (resizeComponent.IsResizing())
				{
					GetViewport().SetInputAsHandled();
				}
			}
		}
    }

    public override void _Notification(int what)
    {
        switch (what)
        {
			case (int)NotificationWMWindowFocusIn:
				break;
            case (int)NotificationWMWindowFocusOut:
				if (resizeComponent.IsResizing())
					resizeComponent.StopResize();
				if (dragComponent.IsDragging())
					dragComponent.StopDrag();
				break;
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.Left)
			{
				if (mb.IsPressed()) {/* do something */}
				else if (dragComponent.IsDragging())
				{
					dragComponent.StopDrag();
				}
			}
		}
		
		if (@event is InputEventMouseMotion m)
		{
			if (m.ButtonMask == MouseButtonMask.Left)
			{
				if (stateComponent.IsMaximized())
				{
					stateComponent.SetWindowed();
					var size = (Vector2I)((Vector2)DisplayServer.WindowGetSize() * 0.5f);
					DisplayServer.WindowSetPosition(DisplayServer.MouseGetPosition() - new Vector2I(size.X, 20));
				}
				if (!dragComponent.IsDragging())
					dragComponent.StartDrag();
			}
		}
    }
}
