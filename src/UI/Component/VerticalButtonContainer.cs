using Godot;

namespace Nasara.UI.Component;

/// <summary>
/// A Vertical Button Container for Navigation,
/// it doesn't have any custom settings yet
/// </summary>
public partial class VerticalButtonContainer : VBoxContainer
{
    [Export] private Panel _highlightPanel;
    [Export] private Color _highlightColor;
    [Export] private float _highlightWidth = 3;
    [Export] private float _edgeSize = 3;

    public ButtonGroup ButtonGroup { get; private set; }

    private int _totalButtons = 0;
    
    // Temporary
    private bool _defaultInit = false;
    private bool _updateNeeded = false;
    private Vector2 _currentPosition = Vector2.Zero;

    public override void _Ready()
    {
        _highlightPanel.SelfModulate = _highlightColor;
        _highlightPanel.Size = new Vector2(_highlightWidth, 0); // X is the width of highlight
        _highlightPanel.Position = _highlightPanel.Position with {X = _edgeSize}; // position to edge
        _highlightPanel.Hide();

        ButtonGroup = new ButtonGroup();
        ButtonGroup.Pressed += DoAnimation; // Animate when switching
    }
    
    public override void _Process(double delta)
    {
        // Okay, WTF is these?
        
        if (!_updateNeeded && !_defaultInit) return;
       
        // FIXME when the container is hiding and adding buttons,
        //       the highlight will get a weird position. (because the position is negative)
        //       try adding button until the container is visible in the tree (loading completed).
        
        // It's a temporary solution
        // bad, but still work right now
        if (_defaultInit && IsVisibleInTree())
        {
            DoAnimation((BaseButton)GetChildren()[0]);
            _defaultInit = false;
        }

        // FIXME error position when resizing
        //       looks like the highlight panel is rendered earlier
        //       than buttons. so it must be updated AFTER button's
        //       position changed.
        
        // TEMPORARY solution!
        // Can I delete this class? it has too many bugs
        if (_updateNeeded && _currentPosition !=
            _highlightPanel.Position with { Y = ButtonGroup.GetPressedButton().GlobalPosition.Y })
        {
            DoAnimation(ButtonGroup.GetPressedButton());
            _updateNeeded = false;
        }
    }

    /// <summary>
    /// Add a button on this container
    /// </summary>
    /// <param name="displayName">Text to display on button</param>
    /// <returns>The button added</returns>
    public Button AddButton(string displayName)
    {
        Button viewButton = new()
        {
            Text = displayName,
            ToggleMode = true,
            ButtonGroup = ButtonGroup,
            Flat = true,
        };
        AddChild(viewButton);
        
        // If it is first button
        if (_totalButtons <= 0)
        {
            // TODO: support no default page
            _highlightPanel.Size = _highlightPanel.Size with { Y = viewButton.Size.Y };
            viewButton.ButtonPressed = true;
            _defaultInit = true;
        }

        _totalButtons++;
        return viewButton;
    }

    /// <summary>
    /// Animate the highlight panel to the given button,
    /// button group will not switch to it
    /// </summary>
    /// <param name="button">The button to switch</param>
    public void DoAnimation(BaseButton button)
    {
        _highlightPanel.Visible = true;

        var tween = CreateTween();
        var newPosition = _highlightPanel.Position with {Y = button.GlobalPosition.Y};
        tween.TweenProperty(_highlightPanel, "position", newPosition, 0.08f).SetEase(Tween.EaseType.In);
        tween.Play();
    }

    public override void _Notification(int what)
    {
        // FIXME error position when resizing (see above)
        switch (what)
        {
            case (int)NotificationResized:
                _currentPosition = _highlightPanel.Position;
                _updateNeeded = true;
                break;
        }
    }
}