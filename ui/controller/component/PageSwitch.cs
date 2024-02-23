using Godot;
using System;

[GlobalClass]
public partial class PageSwitch : Control
{
    [Export]
    Control[] pages;

    [Export]
    public int CurrentPage
    {
        get { return _page;  }
        set { QueueRedraw(); _page = value; }
    }

    int _page = 0;

    public override void _Ready()
    {
        if (pages.Length > 0)
            pages[_page].Show();
    }

    public override void _Draw()
    {
        if (_page >= 0 && pages.Length >= _page + 1)
            for (int i = 0; i < pages.Length; i++)
                if (i == _page)
                    pages[i].Show();
                else
                    pages[i].Hide();
    }

    public void SwitchPage(int index) => CurrentPage = index;
}