using Godot;
using System;

public partial class NavButtonContainer : VBoxContainer
{
    [Export]
    Panel highlightPanel;

    [Export]
    Color highlightColor = new(0.027f, 0.62f, 0.961f);

    public ButtonGroup ButtonGroup {get; private set;}

    int _totalButtons = 0;
    bool _default_init = false;

    public override void _Ready()
    {
        highlightPanel.SelfModulate = highlightColor;
        highlightPanel.Size = new(2, 0);
        highlightPanel.Position = highlightPanel.Position with {X = 2}; //Position.X - 4};
        highlightPanel.Hide();

        ButtonGroup = GD.Load<ButtonGroup>("res://ui/component/nav_button_group.tres");
        ButtonGroup.Pressed += DoAnimation;
    }

    public Button AddButton(string display_name)
    {
        Button viewButton = new()
		{
			Text = display_name,
			ToggleMode = true,
			ButtonGroup = ButtonGroup,
			Flat = true,
		};

        AddChild(viewButton);

        // First button
        if (_totalButtons == 0)
        {
            // TODO: refactor to support no default page
            highlightPanel.Size = highlightPanel.Size with {Y = viewButton.Size.Y};
            viewButton.ButtonPressed = true;
            _default_init = true;
            
            /*GD.Print("this", Position, GlobalPosition, Size, IsVisibleInTree());
            GD.Print("button", viewButton.Position, viewButton.GlobalPosition, viewButton.Size);
            GD.Print("highlight", highlightPanel.Position, highlightPanel.GlobalPosition, highlightPanel.Size);*/
        }

        _totalButtons++;
        return viewButton;
    }

    public override void _Process(double delta)
    {
        // FIXME: when the container is not visible in the tree and it's adding buttons,
        //  the highlight will get a weird position. (because the position is nagative)
        //  try adding button after the container is visible in the tree (completed loading).

        // It's a temporary solution
        if (_default_init && IsVisibleInTree())
        {
            // bad, but still work right now
            DoAnimation((BaseButton)GetChildren()[0]);
            _default_init = false;
        }
    }

    public void DoAnimation(BaseButton button)
    {
        highlightPanel.Visible = true;

        var tween = CreateTween();
        var new_position = highlightPanel.Position with {Y = button.GlobalPosition.Y};
        tween.TweenProperty(highlightPanel, "position", new_position, 0.08f).SetEase(Tween.EaseType.In);
        tween.Play();

        /*GD.Print("button", button.Position, button.GlobalPosition);
        GD.Print("highlight", highlightPanel.Position, highlightPanel.GlobalPosition);
        GD.Print("new_position", new_position);*/
    }
}