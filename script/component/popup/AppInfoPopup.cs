using Godot;
using System;

public partial class AppInfoPopup : Window
{

	[Export]
	Label versionLabel;

	[Export]
	RichTextLabel richTextLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CloseRequested += Hide;

		richTextLabel.MetaClicked += OpenLink;

		versionLabel.Text = "v" + (string)ProjectSettings.GetSetting("application/config/version");
	}

    private void OpenLink(Variant meta)
    {
        string link = (string)meta;
		OS.ShellOpen(link);
    }
}
