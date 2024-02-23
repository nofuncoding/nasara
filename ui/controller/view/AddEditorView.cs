using Godot;
using Semver;
using System;
using System.IO.Compression;
using Humanizer;
using Editor = Nasara.Core.Management.Editor;
using static Nasara.Core.Management.Editor.GodotVersion;
using System.Text;

namespace Nasara.UI.View;

public partial class AddEditorView : PageSwitch
{
	App app;

	Editor.Manager _editorManager;
	Editor.Version versionManager;

	Godot.Collections.Array<Editor.DownloadableVersion> stableVersions = [];
	Godot.Collections.Array<Editor.DownloadableVersion> unstableVersions = [];
	Godot.Collections.Array<Editor.GodotVersion> installedVersions = [];

	[ExportGroup("Pages", "page")] // TODO: Refactor these to make code clean

	[ExportSubgroup("Type Select Page")]
	[Export]
	BaseButton installButton;
	[Export]
	BaseButton importExistingButton;
	[Export]
	BaseButton cancelAddingButton;

	[ExportSubgroup("Install Setting Page")]
	[Export]
	CheckButton monoCheckButton;
	[Export]
	OptionButton channelOption;
	[Export]
	OptionButton versionOption;
	[Export]
	BaseButton continueButton;
	[Export]
	BaseButton backButton;
	[Export]
	Label alreadyInstalled;

	[ExportSubgroup("Install Downloading Page")]
	[Export]
	Label progressLabel;
	[Export]
	ProgressBar progressBar;
	[Export]
	BaseButton finishButton;
	[Export]
	HBoxContainer downloadDisplay;

	[ExportSubgroup("Import Page")]
	[Export]
	LineEdit pathEdit;
	[Export]
	BaseButton explodeButton;
	[Export]
	RichTextLabel resultTextLabel;
	[Export]
	BaseButton importButton;
	[Export]
	BaseButton importBackButton;

	[Signal]
	public delegate void CompletedEventHandler();


	// TODO: Add Cancel when Downloading

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		app = App.Get();
		_editorManager = App.GetEditorManager();
		versionManager = _editorManager.Version;
		installedVersions = Editor.Version.GetVersions();
		SwitchPage(0);

		// Setup Signals
		// Buttons
		installButton.Pressed += () => {
			SwitchPage(1);
			GetGodotList();
		};
		importExistingButton.Pressed += () => SwitchPage(3);
		cancelAddingButton.Pressed += () => EmitSignal(SignalName.Completed); // Don't use Queue Free

		continueButton.Pressed += DownloadTargetVersion;
		backButton.Pressed += () => SwitchPage(0);

		finishButton.Pressed += () => EmitSignal(SignalName.Completed);

		explodeButton.Pressed += () => {
			FileDialog dialog = new() {
				Access = FileDialog.AccessEnum.Filesystem,
				FileMode = FileDialog.FileModeEnum.OpenDir,
				// Title = "Open a Godot Directory",
				// Theme = Theme,
				UseNativeDialog = true, // TODO: Use custom dialog is possible
			};
			dialog.DirSelected += (string dir) => {
				pathEdit.Text = dir;
				CheckPath(dir);
				dialog.QueueFree();
			};
			AddChild(dialog);
			dialog.PopupCentered();
		};
		importButton.Pressed += () => {
			// pageImportExisting.Visible = false;
			versionManager.AddVersion(Editor.Version.PathHasGodot(pathEdit.Text));

			EmitSignal(SignalName.Completed);
		};
		importBackButton.Pressed += () => SwitchPage(0);

		// Options
		channelOption.ItemSelected += (long index) => {
			switch (index)
			{
				case (int)VersionChannel.Stable:
					versionOption.Clear();
					foreach (Editor.DownloadableVersion version in stableVersions)
						versionOption.AddItem(version.Version.ToString());
					break;
				case (int)VersionChannel.Unstable:
					versionOption.Clear();
					foreach (Editor.DownloadableVersion version in unstableVersions)
						versionOption.AddItem(version.Version.ToString());
					break;
			}
			InstallVersionSelected(versionOption.Selected);
		};
		versionOption.ItemSelected += InstallVersionSelected;
		
		pathEdit.TextChanged += CheckPath;

		// Setup styles
		monoCheckButton.ButtonPressed = false;
		finishButton.Visible = false;
		alreadyInstalled.Visible = false;
		downloadDisplay.Visible = false;
		importButton.Disabled = true;
		resultTextLabel.Clear();
	}

	void GetGodotList()
	{
		stableVersions = app.stableVersions;
		unstableVersions = app.unstableVersions;

		versionOption.Clear();
		foreach (Editor.DownloadableVersion version in stableVersions) 
			versionOption.AddItem(version.Version.ToString());
			InstallVersionSelected(0);
	}

	void InstallVersionSelected(long index)
	{
		continueButton.Disabled = false;
		monoCheckButton.Disabled = false;
		monoCheckButton.ButtonPressed = false;
		alreadyInstalled.Visible = false;

		Editor.DownloadableVersion selectedVersion;
		if (channelOption.Selected == (int)VersionChannel.Stable)
			selectedVersion = stableVersions[(int)index]; // Why not use the `Text`?
		else
			selectedVersion = unstableVersions[(int)index];
		
		bool hasMono = false;
		bool hasGds = false;

		foreach (Editor.GodotVersion version in installedVersions)
		{
			if (version.Version.Equals(selectedVersion.Version))
				if (version.Mono)
					hasMono = true;
				else
					hasGds = true;
				//GD.Print(version.Version.ToString(), version.Mono);
		}

		//GD.Print(selectedVersion.Version.ToString(), hasMono, hasGds);

		if (hasMono && hasGds)
		{
			continueButton.Disabled = true;
			monoCheckButton.Disabled = true;
			monoCheckButton.ButtonPressed = false;
			alreadyInstalled.Visible = true;
		} else if (hasMono)
		{
			monoCheckButton.ButtonPressed = false;
			monoCheckButton.Disabled = true;
		} else if (hasGds)
		{
			monoCheckButton.ButtonPressed = true;
			monoCheckButton.Disabled = true;
		}
	}

	void DownloadTargetVersion()
	{
			if (channelOption.Selected == (int)VersionChannel.Stable)
			{ 
				foreach (Editor.DownloadableVersion version in stableVersions)
				{
					if (version.Version.Equals(SemVersion.Parse(versionOption.Text, SemVersionStyles.Any)))
					{
						/*
						// Using Unstable to fix
						if (version.GetDownloadUrl(GetTargetPlatform()) is not null)
						{
							StartDownload(version);
							return;
						}
						else
						{
							GD.PushWarning("[BUG] Get a null url when requesting the latest stable version");
							GD.PushWarning("Failed to Get Download Link from godotengine/godot, Trying to use godotengine/godot-builds");
							
							foreach (DownloadableVersion i in unstableVersions)
								if (i.Version.Equals(SemVersion.Parse(versionOption.Text, SemVersionStyles.Any)))
								{
									if (i.GetDownloadUrl(GetTargetPlatform()) is not null)
									{
										StartDownload(i);
										return;
									}
								}
							
							GD.PushError("Failed to Get Download Link from godotengine/godot-builds again");
						}
						*/
						
						// FIXME: Sometimes null
						if (Editor.Downloader.GetDownloadUrl(version, monoCheckButton.ButtonPressed).Length > 0)
						{
							StartDownload(version);
							return;
						}
					}
				}
			}
			else
			{
				foreach (Editor.DownloadableVersion _version in unstableVersions)
				{
					if (_version.Version.Equals(SemVersion.Parse(versionOption.Text, SemVersionStyles.Any)))
					{
						StartDownload(_version);
						return;
					}
				}
			}
			//StartDownload(unstableVersions[versionOption.Selected]);
		}

	void StartDownload(Editor.DownloadableVersion version)
	{
		SwitchPage(2);
		
		Editor.Downloader downloader = new();
		AddChild(downloader);

		string versionString = version.Version.ToString();
		if (monoCheckButton.ButtonPressed)
			versionString = "Mono " + versionString;
		
		Label sizeLabel = downloadDisplay.GetNode<Label>("SizeLabel");
		Label speedLabel = downloadDisplay.GetNode<Label>("SpeedLabel");

		Timer timer = new()
		{
			WaitTime = 0.1f
		};
		AddChild(timer);

		timer.Timeout += () => {
			int bodySize = downloader.GetBodySize();
			int downloaded = downloader.GetDownloadedBytes();
			if (bodySize != -1)
			{
				// should not put here, but no other places to put this
				if (downloadDisplay.Visible == false)
					downloadDisplay.Visible = true;
				
				progressLabel.Text = string.Format(Tr("Downloading Godot {0}"), versionString); 

				sizeLabel.Text = string.Format("{0}/{1}", downloaded.Bytes().Humanize(), bodySize.Bytes().Humanize());
				speedLabel.Text = $"{downloader.GetSpeedPerSecond().Bytes().Humanize()}/s";

				if (progressBar.MaxValue != bodySize) // do i actually need this
					progressBar.MaxValue = bodySize;
				progressBar.Value = downloaded;
			}

		};

		progressLabel.Text = string.Format(Tr("Requesting Godot {0}"), versionString);
						
		downloader.Download(version, monoCheckButton.ButtonPressed);
		downloader.DownloadFinished += (string savePath) => {
			progressBar.Value = progressBar.MaxValue + 1;
			progressLabel.Text = Tr("Download Completed");
			downloadDisplay.Visible = false;
			VerifyFile(version, savePath);
			
			timer.QueueFree();
		};

		timer.Start();
	}

	async void VerifyFile(Editor.DownloadableVersion version, string savePath)
	{
		var platform = Editor.Downloader.GetTargetPlatform(monoCheckButton.ButtonPressed);
		GD.Print($"Verifying {savePath} for {platform}");
		progressLabel.Text = Tr("Verifying");
		
		savePath = ProjectSettings.GlobalizePath(savePath);

		try 
		{
			var sha512 = await version.GetSha512Async(platform);

			var stream = System.IO.File.OpenRead(savePath);

			var shaM = System.Security.Cryptography.SHA512.Create();
			var result = await shaM.ComputeHashAsync(stream);

			if (sha512 != result.HexEncode())
			{
				GD.PushError($"Verify Failed ({result.HexEncode()[..6]} .. {sha512[..6]})");
				throw new Exception("Failed to Verify File: " + savePath);
			}
			
			GD.Print($"Verify Completed ({result.HexEncode()[..6]} .. {sha512[..6]})");
		}
		catch (Exception e)
		{
			finishButton.Visible = true;
			progressLabel.Text = Tr("Verifying Failed");
			App.GetNotifySystem().Notify(type: NotificationType.Error, title: Tr("Failed to Verify File"), description: e.Message);
			return;
		}

		UnpackGodot(version, savePath);
	}

	void UnpackGodot(Editor.DownloadableVersion version, string zipPath)
	{
		progressLabel.Text = Tr("Unzipping Files");

		/*
		// Unzipping Godot
		ZipReader zip = new();
		if (zip.Open(zipPath) != Error.Ok)
		{
			GD.PushError("Failed to Read Zip: ", zipPath);
			return;
		}

		string[] totalFiles = zip.GetFiles();
		progressBar.MaxValue = totalFiles.Length;

		string path = "user://editors";
		// Special Processing when not mono
		if (!monoCheckButton.ButtonPressed)
			path = path.PathJoin(totalFiles[0].GetBaseName());

		if (!DirAccess.DirExistsAbsolute(path))
			DirAccess.MakeDirRecursiveAbsolute(path);

		foreach (string filePath in totalFiles)
		{
			GD.Print("Unzipping  ", filePath);
			byte[] content = zip.ReadFile(filePath);
			string absFilePath = path.PathJoin(filePath);

			
			using var file = FileAccess.Open(absFilePath, FileAccess.ModeFlags.Write);

			// if not a file, then it's a dir
			if (file is null)
			{
				if (!DirAccess.DirExistsAbsolute(absFilePath))
				{
					GD.Print("Creating   ", filePath);
					DirAccess.MakeDirRecursiveAbsolute(absFilePath);
					progressBar.Value += 1;
					continue;
				}
				
				GD.PushError("Failed to Unzip: ", FileAccess.GetOpenError());
				return;
			}
			
			file.StoreBuffer(content);
			progressBar.Value += 1;
		}

		zip.Close();
		
		// Setting up
		progressLabel.Text = Tr("Completed");

		string editorPath = path.PathJoin(totalFiles[0]);
		if (!monoCheckButton.ButtonPressed)
			editorPath = path;

		GD.Print($"Unzipped to {editorPath}");*/

		string path = ProjectSettings.GlobalizePath("user://editors");

		zipPath = ProjectSettings.GlobalizePath(zipPath);

		// TODO: replace it with a better way.
		var end_dir = "";
		using var arc = ZipFile.OpenRead(zipPath);

		foreach (var entry in arc.Entries)
			if (entry.FullName.StartsWith("Godot") && !entry.FullName.Contains("console"))
			{
				end_dir = entry.FullName;
				break;
			}

		GD.Print($"{zipPath} -> {path}");

		// special processing to get a flat directory
		if (!monoCheckButton.ButtonPressed)
		{ 
			path = path.PathJoin(end_dir);
			ZipFile.ExtractToDirectory(zipPath, path);
		} else {
			ZipFile.ExtractToDirectory(zipPath, path);
			path = path.PathJoin(end_dir);
		}

		// Cleaning up
		DirAccess.RemoveAbsolute(zipPath);

		progressLabel.Text = Tr("Completed");

		FinishInstalling(version, path);
	}

	void FinishInstalling(Editor.DownloadableVersion version, string editorPath)
	{
		// godotManager.AddVersion(new GodotVersion(version.Version, editorPath, version.Channel, monoCheckButton.ButtonPressed));
		// Because of bug above, we use this:
		VersionChannel versionChannel = VersionChannel.Stable;
		if (version.Version.IsPrerelease)
			versionChannel = VersionChannel.Unstable;
		versionManager.AddVersion(new Editor.GodotVersion(version.Version, editorPath, versionChannel, monoCheckButton.ButtonPressed));
		
		if (monoCheckButton.ButtonPressed)
			GD.Print("Added Godot Mono ", version.Version.ToString(), "; Prerelease=", version.Version.IsPrerelease);
		else
			GD.Print("Added Godot ", version.Version.ToString(), "; Prerelease=", version.Version.IsPrerelease);
		
		finishButton.Visible = true;
	}

	void CheckPath(string text) {
		resultTextLabel.Clear();
		resultTextLabel.Text = Tr("Checking") + "...";
		importButton.Disabled = true;

		Editor.GodotVersion ver = Editor.Version.PathHasGodot(text);
		if (ver is null)
		{
			resultTextLabel.Text = $"[color=red][font=res://res/font/MaterialSymbolsSharp.ttf]error[/font] {Tr("Invalid Path. Did you modify the name of the Godot executable?")}[/color]";
		} else {
			if (!Editor.Version.VersionExists(ver))
			{
				resultTextLabel.Text = $"[color=green][font=res://res/font/MaterialSymbolsSharp.ttf]done[/font] {Tr("A Great Path!")}[/color]\n";
				
				// Display the version detected:
				resultTextLabel.AppendText($"Version: [b]{ver.Version}[/b]\n");
				resultTextLabel.AppendText($"Mono: [b]{ver.Mono}[/b]\n");
				resultTextLabel.AppendText($"Channel: [b]{ver.Channel}[/b]\n");

				importButton.Disabled = false;
			}
			else
				resultTextLabel.Text = $"[color=yellow][font=res://res/font/MaterialSymbolsSharp.ttf]warning[/font] {Tr("Version Existed!")}[/color]";
		}
	}
}
