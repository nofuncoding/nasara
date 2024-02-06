using Godot;
using System;

namespace Nasara.UI.View;

public partial class SettingView : Control
{
	AppConfig config = new();

	[Export]
	PanelContainer restartBallon;
	[Export]
	Button restartBallonButton;

	[ExportGroup("App")]
	[Export]
	OptionButton langOption;

	[ExportGroup("Network")]
	[Export]
	CheckButton enableTLS;
	[Export]
	CheckButton githubProxy;

	[ExportGroup("Theme")]
	[Export]
	CheckButton transparent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		restartBallon.Visible = false;
		config.NeedRestart += () => restartBallon.Visible = true;

		restartBallonButton.Pressed += () => {
			// GetTree().ReloadCurrentScene(); // Reload the `App` Scene only
			OS.ShellOpen(OS.GetExecutablePath()); // Restart the whole Application
			GetTree().Quit();
		};

		/* Init Options */

		InitLanguageOptions();

		enableTLS.ButtonPressed = config.EnableTLS;
		githubProxy.ButtonPressed = config.UsingGithubProxy;

		transparent.ButtonPressed = config.TransparentBackground;
		
		/* Events */

		langOption.ItemSelected += (long index) => config.Language = TranslationServer.GetLoadedLocales()[index];
		
		enableTLS.Toggled += (bool s) => config.EnableTLS = s;
		githubProxy.Toggled += (bool s) => config.UsingGithubProxy = s;
	
		transparent.Toggled += (bool s) => {
			config.TransparentBackground = s;
			GetTree().Root.TransparentBg = s;
		};
	}

	void InitLanguageOptions()
	{
		foreach (string locale in TranslationServer.GetLoadedLocales())
		{
			langOption.AddItem(TranslationServer.GetLanguageName(locale));
			if (locale == TranslationServer.GetLocale())
				langOption.Select(langOption.ItemCount - 1);
		}
	}
}