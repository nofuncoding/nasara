using Godot;
using System;

namespace Nasara.UI.View;

public partial class SettingView : Control
{
	AppConfig _config = new();

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
	[Export]
	CheckButton customTitlebar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		restartBallon.Visible = false;
		_config.NeedRestart += () => restartBallon.Visible = true;

		restartBallonButton.Pressed += () => {
			// GetTree().ReloadCurrentScene(); // Reload the `App` Scene only
			OS.ShellOpen(OS.GetExecutablePath()); // Restart the whole Application
			GetTree().Quit();
		};

		/* Init Options */

		InitLanguageOptions();

		enableTLS.ButtonPressed = _config.EnableTLS;
		githubProxy.ButtonPressed = _config.UsingGithubProxy;

		transparent.ButtonPressed = _config.TransparentBackground;
		customTitlebar.ButtonPressed = _config.UseCustomTitlebar;
		
		/* Events */

		langOption.ItemSelected += (long index) => _config.Language = TranslationServer.GetLoadedLocales()[index];
		
		enableTLS.Toggled += (bool s) => _config.EnableTLS = s;
		githubProxy.Toggled += (bool s) => _config.UsingGithubProxy = s;
	
		transparent.Toggled += (bool s) => {
			_config.TransparentBackground = s;
			GetTree().Root.TransparentBg = s;
		};
		customTitlebar.Toggled += (bool s) => _config.UseCustomTitlebar = s;
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