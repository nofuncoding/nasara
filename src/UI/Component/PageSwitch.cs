using Godot;

namespace Nasara.UI.Component;

[GlobalClass]
public partial class PageSwitch : Control
{
    [Export] protected Control[] Pages;
    [Export] public int CurrentPageIndex { get => _currentPageIndex;
        set
        {
            QueueRedraw();
            _currentPageIndex = value;
        }
    }

    private int _currentPageIndex = 0;

    public override void _Ready()
    {
        // If it has page, then show the specified page index
        if (Pages.Length > 0)
            Pages[_currentPageIndex].Show();
    }

    public override void _Draw()
    {
        if (_currentPageIndex < 0 || Pages.Length < _currentPageIndex + 1) return;
        
        for (var i = 0; i < Pages.Length; i++)
            if (i == _currentPageIndex)
                Pages[i].Show(); // Show the current page
            else
                Pages[i].Hide(); // Hide the others
    }
}