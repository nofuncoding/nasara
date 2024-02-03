using Godot;
using System;

namespace Nasara.UI.Component;

public partial class NewsHolder : PanelContainer
{
	[Export]
	RichTextLabel titleRtl; // TODO: Use `LinkButton` instead
	[Export]
	Label descriptionLabel;
	[Export]
	Label pubDateLabel;
	[Export]
	TextureRect imageRect;

	public override void _Ready()
	{
		titleRtl.MetaUnderlined = false;
		titleRtl.MetaClicked += (Variant meta) => { OS.ShellOpen((string)meta); };
		titleRtl.MetaHoverStarted += (Variant _) => {
			DisplayServer.CursorSetShape(DisplayServer.CursorShape.PointingHand);
			titleRtl.MetaUnderlined = true;
		};
		titleRtl.MetaHoverEnded += (Variant _) => {
			DisplayServer.CursorSetShape(DisplayServer.CursorShape.Arrow);
			titleRtl.MetaUnderlined = false;
		};

		titleRtl.Clear();
		descriptionLabel.Text = "";
		pubDateLabel.Text = "";
	}

	public void SetTitle(string title)
	{
		titleRtl.AddText(title);
	}

	public void SetDescription(string description)
	{
		descriptionLabel.Text = description;
	}

	public void SetTexture(ImageTexture texture)
	{
		imageRect.Texture = texture;
	}

	public void SetLink(string link)
	{
		var original = titleRtl.GetParsedText();
		titleRtl.Clear();

		titleRtl.PushMeta(link);
		titleRtl.AddText(original);
		titleRtl.Pop();
	}

	public void SetPubDate(string pubDate)
	{
		pubDateLabel.Text = pubDate;
	}
}
