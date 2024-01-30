using Godot;
using System;

namespace Nasara.UI.Component.Titlebar;

public partial class ResizeComponent : Node
{
	[Signal]
	public delegate void CursorIndexChangedEventHandler(int index);

	[Export]
	int WindowId = 0;
	[Export]
	public bool Activate { get { return Activate; } set { SetProcess(value); } }
	/// <summary>
	/// min window size
	/// </summary>
	[Export]
	Vector2I MinSize = new(260, 100);
	/// <summary>
	/// distance (pixel) to window border on detect state
	/// </summary>
	[Export]
	int DetectStateGap = 2;
	/// <summary>
	/// distance (pixel) to window border on other state
	/// </summary>
	[Export]
	int OtherStateGap = 16;
	/// <summary>
	/// set to false if you don't want to change cursor
	/// </summary>
	[Export]
	bool ChangeCursor = true;
	/// <summary>
	/// set to false if you want to change cursor on fullscreen mode
	/// </summary>
	[Export]
	bool OnlyChangeOnWindowedMode = true;

	static DisplayServer.CursorShape[] CURSOR_LIST = {
		DisplayServer.CursorShape.Arrow,
		DisplayServer.CursorShape.Fdiagsize,
		DisplayServer.CursorShape.Vsize,
		DisplayServer.CursorShape.Bdiagsize,
		DisplayServer.CursorShape.Hsize,
		DisplayServer.CursorShape.Arrow,
		DisplayServer.CursorShape.Hsize,
		DisplayServer.CursorShape.Bdiagsize,
		DisplayServer.CursorShape.Vsize,
		DisplayServer.CursorShape.Fdiagsize,
	};

	enum State {
		Detect,
		Wait,
		Resize,
	}
	State state = State.Detect;

	Rect2I startWindowRect;
	int resizeIndex = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Activate = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (OnlyChangeOnWindowedMode && DisplayServer.WindowGetMode(WindowId) != DisplayServer.WindowMode.Windowed)
			return;

		var mousePos = DisplayServer.MouseGetPosition();
		var windowRect = GetWindowRect(WindowId);
		var gap = state == State.Detect ? DetectStateGap : OtherStateGap;
		var insideRect = windowRect.Grow(-gap);

		if (state != State.Resize)
			resizeIndex = GetGridIndex(windowRect, mousePos, gap);
		EmitSignal(SignalName.CursorIndexChanged, resizeIndex);
		if (ChangeCursor)
			DisplayServer.CursorSetShape(CURSOR_LIST[resizeIndex]);

		switch (state)
		{
			case State.Detect:
				if (windowRect.HasPoint(mousePos) && !insideRect.HasPoint(mousePos))
					state = State.Wait;
				break;
			case State.Wait:
				if (windowRect.HasPoint(mousePos) && !insideRect.HasPoint(mousePos))
					startWindowRect = windowRect;
				else
					state = State.Detect;
				break;
			case State.Resize:
				ResizeWindow(startWindowRect, mousePos, resizeIndex, MinSize);
				break;
		}
	}

	/* Interface Methods */

	public bool IsWaitingResize()
	{
		return state == State.Wait;
	}

	public bool IsResizing()
	{
		return state == State.Resize;
	}
	
	public void StartResize()
	{
		if (state == State.Wait)
			state = State.Resize;
	}

	public void StopResize()
	{
		if (state == State.Resize)
			state = State.Detect;
	}

	void ResizeWindow(Rect2I startRect, Vector2I pos, int index, Vector2 minSize, int windowId = 0)
	{
		var rect = (Rect2I)ResizeRect(startRect, pos, index, minSize);
		DisplayServer.WindowSetPosition(rect.Position, windowId);
		DisplayServer.WindowSetSize(rect.Size, windowId);
	}

	/* Static Methods */

	static Rect2 ResizeRect(Rect2I rect, Vector2I pos, int index, Vector2 minSize)
	{
		var start = rect.Position;
		var end = rect.End;

		switch (index)
		{
			case 1:
				start = pos; break;
			case 2:
				start.Y = pos.Y; break;
			case 3:
				start.Y = pos.Y;
				end.X = pos.X;
				break;
			case 4:
				start.X = pos.X; break;
			case 6:
				end.X = pos.X; break;
			case 7:
				start.X = pos.X;
				end.Y = pos.Y;
				break;
			case 8:
				end.Y = pos.Y; break;
			case 9:
				end = pos; break;
		}
		var newSize = end - start;
		if (newSize.X < minSize.X)
		{
			if (index == 1 || index == 4 || index == 7)
				start.X = end.X - (int)minSize.X;
		}
		if (newSize.Y < minSize.Y)
		{
			if (index == 1 || index == 2 || index == 3)
				start.Y = end.Y - (int)minSize.Y;
		}

		newSize.X = Math.Max((int)minSize.X, newSize.X);
		newSize.Y = Math.Max((int)minSize.Y, newSize.Y);

		return new Rect2(start, newSize);
	}

	static int GetGridIndex(Rect2I windowRect, Vector2I mousePos, int gap)
	{
		var local_mousePos = mousePos - windowRect.Position;
		var width = windowRect.Size.X;
		var height = windowRect.Size.Y;
		var x = local_mousePos.X;
		var y = local_mousePos.Y;

		var index = 0;

		if (y < gap)
		{
			if (x < gap)
				index = 1;
			else if (gap <= x && x < width-gap)
				index = 2;
			else if (width-gap <= x)
				index = 3;
		}
		else if (gap <= y && y < height-gap)
		{
			if (x < gap)
				index = 4;
			else if (gap <= x && x < width-gap)
				index = 5;
			else if (width-gap <= x)
				index = 6;
		}
		else if (height-gap <= y)
		{
			if (x < gap)
				index = 7;
			else if (gap <= x && x < width-gap)
				index = 8;
			else if (width-gap <= x)
				index = 9;
		}

		return index;
	}

	static Rect2I GetWindowRect(int window_id = 0)
	{
		return new(DisplayServer.WindowGetPosition(window_id), DisplayServer.WindowGetSize(window_id));
	}
}
