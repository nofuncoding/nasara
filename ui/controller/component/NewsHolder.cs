using Godot;
using System;

namespace Nasara.UI.Component;

public partial class NewsHolder : PanelContainer
{
	[Export]
	LinkButton titleButton;
	[Export]
	Label descriptionLabel;
	[Export]
	Label pubDateLabel;
	[Export]
	TextureRect imageRect;

	public override void _Ready()
	{
		titleButton.Text = "";
		descriptionLabel.Text = "";
		pubDateLabel.Text = "";
	}

	public void SetTitle(string title) => titleButton.Text = title;
	
	public void SetDescription(string description) => descriptionLabel.Text = description;

	public void SetTexture(ImageTexture texture) => imageRect.Texture = texture;

	public void SetLink(string link) => titleButton.Uri = link;

	public void SetPubDate(string pubDate) => pubDateLabel.Text = pubDate;
}
