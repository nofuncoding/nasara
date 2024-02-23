using Godot;
using System;
using Project = Nasara.Core.Management.Project;

namespace Nasara.UI.View;

public partial class AddProjectView : PageSwitch
{
    App _app;
    Project.Manager _projectManager;

    [ExportGroup("Type Select")]
    [Export]
    BaseButton typeImportButton;
    [Export]
    BaseButton typeExitButton;
    [ExportGroup("Import")]
    [Export]
    LineEdit importPathEdit;
    [Export]
    BaseButton importOpenButton;
    [Export]
	RichTextLabel importResultLabel;
	[Export]
	BaseButton importButton;
	[Export]
	BaseButton importBackButton;

    [Signal]
    public delegate void CompletedEventHandler();

    public override void _Ready()
    {
        _app = App.GetInstance();
        _projectManager = App.GetProjectManager();

        typeImportButton.Pressed += () => SwitchPage(1);
        typeExitButton.Pressed += () => EmitSignal(SignalName.Completed);
    
        importPathEdit.TextChanged += CheckPath;
        importPathEdit.TextSubmitted += (string text) => {
            if (!importButton.Disabled)
            {
                _projectManager.Add(new(text));

			    EmitSignal(SignalName.Completed);
            }
        };
        importOpenButton.Pressed += () => {
			FileDialog dialog = new() {
				Access = FileDialog.AccessEnum.Filesystem,
				FileMode = FileDialog.FileModeEnum.OpenDir,
				// Title = "Open a Godot Directory",
				// Theme = Theme,
				UseNativeDialog = true, // TODO: Use custom dialog if possible
			};
			dialog.DirSelected += (string dir) => {
				importPathEdit.Text = dir;
				CheckPath(dir);
				dialog.QueueFree();
			};
			AddChild(dialog);
			dialog.PopupCentered();
		};
        importBackButton.Pressed += () => SwitchPage(0);
        importButton.Disabled = true;
        importButton.Pressed += () => {
			_projectManager.Add(new(importPathEdit.Text));

			EmitSignal(SignalName.Completed);
		};
    }

    void CheckPath(string path)
    {
        importResultLabel.Clear();
		importResultLabel.Text = Tr("Checking") + "...";
		importButton.Disabled = true;

        Project.Project p;

        try
        {
            p = new(path);
        }
        catch
        {
            importResultLabel.Text = $"[color=red][font=res://res/font/MaterialSymbolsSharp.ttf]error[/font] {Tr("Invalid Project Path.")}[/color]";
            return;
        }

        if (_projectManager.ProjectExists(p))
        {
            importResultLabel.Text = $"[color=yellow][font=res://res/font/MaterialSymbolsSharp.ttf]warning[/font] {Tr("Project Existed!")}[/color]";
            return;
        }

        importResultLabel.Text = $"[color=green][font=res://res/font/MaterialSymbolsSharp.ttf]done[/font] {Tr("A Great Path!")}[/color]\n";
				
        // Display the version detected:
        importResultLabel.AppendText($"Name: [b]{p.Name}[/b]\n");
        importResultLabel.AppendText($"Editor Version: [b]{p.UsingGodotVersion}[/b]\n");

        importButton.Disabled = false;
    }
}
