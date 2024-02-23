using Godot;
using System;

namespace Nasara.UI.Component;

[GlobalClass]
public partial class NotifyBallon : Control
{
	[Export]
	Label IconLabel;

	[Export]
	Label TitleLabel;

	[Export]
	Label DescriptionLabel;

	[Export]
	ProgressBar TimerBar;

	[Export]
	Button HideButton;

	/// <summary>
	/// If it set to 0, the ballon won't auto hide
	/// </summary>
	[Export]
	public float HideTime = 8f;

	[Export]
	float FadeInOutTime = 0.3f;

	Tween tween;

	Vector2 _originalPosition;
	Vector2 _hidePosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// FIXME: Currently, fade in/out is not working if `clip children` is true
		// Maybe an error from Godot.
		
		// SelfModulate = new Color(1, 1, 1, 0);
		_originalPosition = Position;
		_hidePosition = new Vector2(_originalPosition.X + Size.X + 50, _originalPosition.Y);
		Position = _hidePosition;

		HideButton.Pressed += HideNotify;

		// var timer = GetTree().CreateTimer(HideTime);
		tween = GetTree().CreateTween();
		tween.Finished += () => QueueFree();

		// tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), FadeInOutTime);
		// tween.TweenProperty(this, "position", _originalPosition, FadeInOutTime); NOT WORKING !!
		if (HideTime > 0)
		{
			tween.TweenProperty(TimerBar, "value", 0, HideTime);
			tween.TweenProperty(this, "position", _hidePosition, FadeInOutTime);
		}
		tween.Play();
	}

	void HideNotify()
	{
		HideButton.Hide();
		tween.Stop();
		tween = null;

		tween = GetTree().CreateTween();
		tween.Finished += () => QueueFree();
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), FadeInOutTime);
		tween.Play();
	}

	public void SetNotifyType(NotificationType type)
	{
		// Defined in Material Symbols, will be replaced with
		// the target symbol using OpenType features
		const string INFO_ICON = "info";
		const string WARN_ICON = "warning";
		const string ERROR_ICON = "error";

		switch (type)
		{
			case NotificationType.Info:
				IconLabel.Text = INFO_ICON;
				IconLabel.LabelSettings = GD.Load<LabelSettings>("res://res/style/notify/ballon_white_icon.tres");
				SetTimerBarColor(new Color(1, 1, 1));
				break;
			case NotificationType.Warn:
				IconLabel.Text = WARN_ICON;
				IconLabel.LabelSettings = GD.Load<LabelSettings>("res://res/style/notify/ballon_yellow_icon.tres");
				SetTimerBarColor(new Color(0.98f, 0.8f, 0.082f));
				break;
			case NotificationType.Error:
				IconLabel.Text = ERROR_ICON;
				IconLabel.LabelSettings = GD.Load<LabelSettings>("res://res/style/notify/ballon_red_icon.tres");
				SetTimerBarColor(new Color(0.863f, 0.149f, 0.149f));
				break;
		}
	}

	public void SetTitle(string title)
	{
		TitleLabel.Text = title;
	}

	public void SetDescription(string description)
	{
		DescriptionLabel.Text = description;
	}

	void SetTimerBarColor(Color color)
	{
		StyleBoxFlat newStyleboxNormal = TimerBar.GetThemeStylebox("fill").Duplicate() as StyleBoxFlat;
		newStyleboxNormal.BgColor = color;

		if(TimerBar.HasThemeStyleboxOverride("fill"))
			TimerBar.RemoveThemeStyleboxOverride("fill");
		
		TimerBar.AddThemeStyleboxOverride("fill", newStyleboxNormal);
	}
}
