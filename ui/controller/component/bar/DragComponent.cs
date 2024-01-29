using Godot;
using System;

namespace Nasara.UI.Component.Titlebar;

public partial class DragComponent : Node
{
	[Export]
	int WindowId = 0;

	[Signal]
	public delegate void DragStartedEventHandler();
	[Signal]
	public delegate void DragStoppedEventHandler();

	Vector2I offsetPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StopDrag();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		DisplayServer.WindowSetPosition(DisplayServer.MouseGetPosition() + offsetPosition, WindowId);
	}

	public void StartDrag()
	{
		SetProcess(true);
		offsetPosition = DisplayServer.WindowGetPosition(WindowId) - DisplayServer.MouseGetPosition();
		EmitSignal(SignalName.DragStarted);
	}

	public void StopDrag()
	{
		SetProcess(false);
		offsetPosition = Vector2I.Zero;
		EmitSignal(SignalName.DragStopped);
	}

	public bool IsDragging()
	{
		return IsProcessing();
	}
}
