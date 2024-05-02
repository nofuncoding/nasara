using Godot;

namespace Nasara.UI.View;

public partial class AppInfo : Window
{
    [Export] private Label _versionLabel;
    [Export] private Label _versionDetail;
    [Export] private RichTextLabel _licensingText;

    public override void _Ready()
    {
        CloseRequested += QueueFree;
        _licensingText.MetaClicked += (meta) => OS.ShellOpen((string)meta);

        _versionLabel.Text = $"Nasara v{App.GetVersion()}";

        var engineInfo = Engine.GetVersionInfo();
        _versionDetail.Text = $"Nasara v{App.GetVersion()} ({Engine.GetArchitectureName()})" + 
                              (OS.IsDebugBuild() ? " (Debug Build)\n" : "\n") + 
                              $"Data directory: {OS.GetUserDataDir()}\n" +
                              $"System Language: {OS.GetLocaleLanguage()}\n" + 
                              $"Built with Godot {engineInfo["string"]}";
    }
}