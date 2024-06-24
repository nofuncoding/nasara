using Godot;

namespace Nasara.UI.Component;

/// <summary>
/// A simplified version of Godot's TabContainer
/// </summary>
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
        // If it has page, then show the default page index
        if (Pages.Length > 0)
            Pages[_currentPageIndex].Show();
    }

    public override void _Draw()
    {
        if (_currentPageIndex < 0)
            foreach (var page in Pages)
            {
                page.Hide();
            }
        
        if (Pages.Length < _currentPageIndex + 1) return;
        
        for (var i = 0; i < Pages.Length; i++)
            if (i == _currentPageIndex)
                Pages[i].Show(); // Show the current page
            else
                Pages[i].Hide(); // Hide the others
    }
}