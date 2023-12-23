using Godot;
using System;

public partial class SettingView : Control
{
	AppConfig config = new();

	[Export]
	PanelContainer restartBallon;
	[Export]
	Button restartBallonButton;

	[ExportGroup("General")]
	[Export]
	OptionButton langOption;

	[ExportGroup("Network")]
	[Export]
	CheckButton enableTLS;
	[Export]
	CheckButton githubProxy;


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

		enableTLS.ButtonPressed = config.EnableTLS;
		githubProxy.ButtonPressed = config.UsingGithubProxy;

		foreach (string locale in TranslationServer.GetLoadedLocales())
		{
			langOption.AddItem(TranslationServer.GetLanguageName(locale));
			if (locale == TranslationServer.GetLocale())
				langOption.Select(langOption.ItemCount - 1);
		}
		

		enableTLS.Toggled += (bool s) => config.EnableTLS = s;
		githubProxy.Toggled += (bool s) => config.UsingGithubProxy = s;
		langOption.ItemSelected += (long index) => {
			string lang = TranslationServer.GetLoadedLocales()[index];
			config.Language = lang;
			// TranslationServer.SetLocale();
		};
	}
}
