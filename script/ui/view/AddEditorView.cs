using Godot;
using Semver;
using System;
using Humanizer;
using Nasara.GodotManager;

namespace Nasara.UI.View;

public partial class AddEditorView : Control
{
	App app;

	GodotManager.Manager godotManager;
	GodotManager.Version versionManager;

	Godot.Collections.Array<DownloadableVersion> stableVersions = new();
	Godot.Collections.Array<DownloadableVersion> unstableVersions = new();
	Godot.Collections.Array<GodotVersion> installedVersions = new();

	[ExportGroup("Pages", "page")] // TODO: Refactor these to make code clean
	[Export]
	Control pageInstallType;
	[Export]
	Control pageInstallSetting;
	[Export]
	Control pageInstallDownloading;
	[Export]
	Control pageImportExisting;

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
	public delegate void AddedEditorEventHandler();


	// TODO: Add Cancel when Downloading

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		app = GetNode<App>("/root/App");
		godotManager = GetNode<GodotManager.Manager>("/root/GodotManager");
		versionManager = godotManager.Version();
		installedVersions = versionManager.GetVersions();
		SwitchView(0);
		

		// Setup Signals
		// Buttons
		installButton.Pressed += () => {
			SwitchView(1);
			GetGodotList();
		};
		importExistingButton.Pressed += () => SwitchView(3);
		cancelAddingButton.Pressed += () => EmitSignal(SignalName.AddedEditor); // Don't use Queue Free

		continueButton.Pressed += DownloadTargetVersion;
		backButton.Pressed += () => SwitchView(0);

		finishButton.Pressed += () => EmitSignal(SignalName.AddedEditor);

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
			pageImportExisting.Visible = false;
			versionManager.AddVersion(GodotManager.Version.PathHasGodot(pathEdit.Text));

			EmitSignal(SignalName.AddedEditor);
		};
		importBackButton.Pressed += () => SwitchView(0);

		// Options
		channelOption.ItemSelected += (long index) => {
			switch (index)
			{
				case (int)GodotVersion.VersionChannel.Stable:
					versionOption.Clear();
					foreach (DownloadableVersion version in stableVersions)
						versionOption.AddItem(version.Version.ToString());
					break;
				case (int)GodotVersion.VersionChannel.Unstable:
					versionOption.Clear();
					foreach (DownloadableVersion version in unstableVersions)
						versionOption.AddItem(version.Version.ToString());
					break;
			}
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
		foreach (DownloadableVersion version in stableVersions) 
			versionOption.AddItem(version.Version.ToString());
			InstallVersionSelected(0);
	}

	void InstallVersionSelected(long index)
	{
		continueButton.Disabled = false;
		monoCheckButton.Disabled = false;
		monoCheckButton.ButtonPressed = false;
		alreadyInstalled.Visible = false;

		DownloadableVersion selectedVersion;
		if (channelOption.Selected == (int)GodotVersion.VersionChannel.Stable)
			selectedVersion = stableVersions[(int)index]; // Why not use the `Text`?
		else
			selectedVersion = unstableVersions[(int)index];
		
		bool hasMono = false;
		bool hasGds = false;

		foreach (GodotVersion version in installedVersions)
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

	void SwitchView(int index)
	{
		switch (index)
		{
			case 0:
				pageInstallType.Visible = true;
				pageInstallSetting.Visible = false;
				pageInstallDownloading.Visible = false;
				pageImportExisting.Visible = false;
				break;
			case 1:
				pageInstallType.Visible = false;
				pageInstallSetting.Visible = true;
				pageInstallDownloading.Visible = false;
				pageImportExisting.Visible = false;
				break;
			case 2:
				pageInstallType.Visible = false;
				pageInstallSetting.Visible = false;
				pageInstallDownloading.Visible = true;
				pageImportExisting.Visible = false;
				break;
			case 3:
				pageInstallType.Visible = false;
				pageInstallSetting.Visible = false;
				pageInstallDownloading.Visible = false;
				pageImportExisting.Visible = true;
				break;
		}
	}

	void DownloadTargetVersion()
	{
			if (channelOption.Selected == (int)GodotVersion.VersionChannel.Stable)
			{ 
				foreach (DownloadableVersion version in stableVersions)
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

						if (new Downloader().GetDownloadUrl(version, monoCheckButton.ButtonPressed).Length > 0)
						{
							StartDownload(version);
							return;
						}
					}
				}
			}
			else
			{
				foreach (DownloadableVersion _version in unstableVersions)
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

	void StartDownload(DownloadableVersion version)
	{
		SwitchView(2);
		
		Downloader downloader = new();
		AddChild(downloader);

		string versionString = version.Version.ToString();
		if (monoCheckButton.ButtonPressed)
			versionString = "Mono " + versionString;
		
		Label sizeLabel = downloadDisplay.GetNode<Label>("SizeLabel");
		Label speedLabel = downloadDisplay.GetNode<Label>("SpeedLabel");

		Timer timer = new()
		{
			WaitTime = 0.1
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

			UnpackGodot(version, savePath);
			timer.QueueFree();
		};

		timer.Start();
	}

	void VerifyFile()
	{
		// TODO: Verifying Files using Sha-512
		// Use System.Security.Cryptography.SHA512
	}

	void UnpackGodot(DownloadableVersion version, string zipPath)
	{
		progressBar.Value = 0;
		progressLabel.Text = Tr("Unzipping Files");

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
				/*
				GD.PushError("Failed to Unzip: ", FileAccess.GetOpenError());
				return;
				*/
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

		GD.Print($"Unzipped to {editorPath}");

		FinishInstalling(version, zipPath, editorPath);
	}

	void FinishInstalling(DownloadableVersion version, string zipPath, string editorPath)
	{
		// godotManager.AddVersion(new GodotVersion(version.Version, editorPath, version.Channel, monoCheckButton.ButtonPressed));
		// Because of bug above, we use this:
		GodotVersion.VersionChannel versionChannel = GodotVersion.VersionChannel.Stable;
		if (version.Version.IsPrerelease)
			versionChannel = GodotVersion.VersionChannel.Unstable;
		versionManager.AddVersion(new GodotVersion(version.Version, editorPath, versionChannel, monoCheckButton.ButtonPressed));

		// Cleaning up
		DirAccess.RemoveAbsolute(zipPath);
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

		GodotVersion ver = GodotManager.Version.PathHasGodot(text);
		if (ver is null)
		{
			resultTextLabel.Text = $"[color=red][font=res://asset/font/MaterialSymbolsSharp.ttf]error[/font] {Tr("Invalid Path. Did you modify the name of the Godot executable?")}[/color]";
		} else {
			if (!versionManager.VersionExists(ver))
			{
				resultTextLabel.Text = $"[color=green][font=res://asset/font/MaterialSymbolsSharp.ttf]done[/font] {Tr("A Great Path!")}[/color]\n";
				
				// Display the version detected:
				resultTextLabel.AppendText($"Version: [b]{ver.Version}[/b]\n");
				resultTextLabel.AppendText($"Mono: [b]{ver.Mono}[/b]\n");
				resultTextLabel.AppendText($"Channel: [b]{ver.Channel}[/b]\n");

				importButton.Disabled = false;
			}
			else
				resultTextLabel.Text = $"[color=yellow][font=res://asset/font/MaterialSymbolsSharp.ttf]warning[/font] {Tr("Version Existed!")}[/color]";
		}
	}
}
