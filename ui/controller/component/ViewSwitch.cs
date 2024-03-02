using Godot;
using System;
using System.Linq;

namespace Nasara.UI.Component;

public partial class ViewSwitch : PageSwitch
{
	public int AddView(PackedScene packedScene)
	{
		var node = packedScene.Instantiate<Control>();
		AddChild(node);
		pages = [.. pages, node];

		return pages.Length - 1;
	}
}
