using Godot;
using System;

namespace Nasara.UI.Component.Titlebar;

public partial class StateComponent : Node
{
	[Export]
	int WindowId = 0;
	[Export]
	Vector2I MaximizePositionCompensatory = new(0, 2);
	[Export]
	Vector2I MaximizeSizeCompensatory = new(0, 48);

	Rect2I prevRect;
	bool maximized = false;

	public bool IsFullscreen()
	{
		return DisplayServer.WindowGetMode(WindowId) == DisplayServer.WindowMode.Fullscreen;
	}

	public bool IsMaximized()
	{
		if (!IsBorderless())
			return DisplayServer.WindowGetMode(WindowId) == DisplayServer.WindowMode.Maximized;
		else
			return maximized;
	}

	public void SetMinimized()
	{
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized, WindowId);
	}

	public void SetMaximized()
	{
		prevRect = GetWindowRect(WindowId);
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized, WindowId);

		if (IsBorderless())
		{
			var rect = GetWindowRect(WindowId);
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed, WindowId);
			DisplayServer.WindowSetPosition(rect.Position + MaximizePositionCompensatory, WindowId);
			DisplayServer.WindowSetSize(rect.Size + MaximizeSizeCompensatory, WindowId);
			maximized = true;
		}
	}

	public void SetFullscreen()
	{
		prevRect = GetWindowRect(WindowId);
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen, WindowId);
	}

	public void SetWindowed()
	{
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed, WindowId);
		if (IsBorderless())
			maximized = false;
		if (prevRect.Position != Vector2I.Zero || prevRect.Size != Vector2I.Zero)
		{
			DisplayServer.WindowSetPosition(prevRect.Position, WindowId);
			DisplayServer.WindowSetSize(prevRect.Size, WindowId);
		}
	}

	public void ToggleFullscreen()
	{
		if (IsFullscreen())
			SetWindowed();
		else
			SetFullscreen();
	}

	public void ToggleMaximized()
	{
		if (IsMaximized())
			SetWindowed();
		else
			SetMaximized();
	}

	public void Quit()
	{
		GetTree().Quit();
	}

	Rect2I GetWindowRect(int windowId)
	{
		return new Rect2I(DisplayServer.WindowGetPosition(windowId), DisplayServer.WindowGetSize(windowId));
	}

	static bool IsBorderless()
	{
		return ProjectSettings.GetSetting("display/window/size/borderless", false).AsBool();
	}
}
