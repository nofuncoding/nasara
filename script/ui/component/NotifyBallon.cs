using Godot;
using System;

namespace Nasara.UI.Component
{
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

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Modulate = new Color(1, 1, 1, 0); // For animation

			HideButton.Pressed += HideNotify;

			// var timer = GetTree().CreateTimer(HideTime);
			tween = GetTree().CreateTween();
			tween.Finished += () => QueueFree();

			tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), FadeInOutTime);
			if (HideTime > 0)
			{
				tween.TweenProperty(TimerBar, "value", 0, HideTime);
				tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), FadeInOutTime);
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
			switch (type)
			{
				case NotificationType.Info:
					IconLabel.Text = "info"; // Set icon
					IconLabel.LabelSettings = GD.Load<LabelSettings>("res://asset/style/notify/ballon_white_icon.tres");
					SetTimerBarColor(new Color(1, 1, 1));
					break;
				case NotificationType.Warn:
					IconLabel.Text = "warning";
					IconLabel.LabelSettings = GD.Load<LabelSettings>("res://asset/style/notify/ballon_yellow_icon.tres");
					SetTimerBarColor(new Color(0.98f, 0.8f, 0.082f));
					break;
				case NotificationType.Error:
					IconLabel.Text = "error";
					IconLabel.LabelSettings = GD.Load<LabelSettings>("res://asset/style/notify/ballon_red_icon.tres");
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
}