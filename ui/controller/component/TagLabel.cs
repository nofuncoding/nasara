using Godot;
using System;

namespace Nasara.UI.Component;

public partial class TagLabel : PanelContainer
{
    [Export]
    string text;
    [Export]
    Color tagColor = new("2e2e2e");

    public override void _Ready()
    {
        if (text is not null)
            GetNode<Label>("Label").Text = text;
        
        var stylebox = GetThemeStylebox("panel") as StyleBoxFlat;
        if (tagColor != stylebox.BgColor)
            stylebox.BgColor = tagColor;
    }
}
