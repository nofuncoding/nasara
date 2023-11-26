using Godot;
using Godot.Collections;
using Semver;
using System;
using System.Linq;

public partial class GodotRequester : Node
{
    // GH Api URLs
    const string GODOT_STABLE = "https://api.github.com/repos/godotengine/godot/releases";
	const string GODOT_UNSTABLE = "https://api.github.com/repos/godotengine/godot-builds/releases";

	[Signal]
	public delegate void RequestCompletedEventHandler(Array<DownloadableVersion> downloadableVersions, int channel);

    public override void _Ready()
    {
		
    }

    public Error RequestEditorList(GodotVersion.VersionChannel channel = GodotVersion.VersionChannel.Stable)
	{
		HttpRequest http = new();
		AddChild(http);

		if (!IsNodeReady())
			return Error.Busy;

		http.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body)
							=> { GodotRequestCompleted(channel, result, responseCode, headers, body); http.QueueFree(); };

		switch (channel)
		{
			case GodotVersion.VersionChannel.Stable:
				http.Request(GODOT_STABLE); GD.Print("GET ", GODOT_STABLE); break;
			case GodotVersion.VersionChannel.Unstable:
				http.Request(GODOT_UNSTABLE); GD.Print("GET ", GODOT_UNSTABLE); break;
		}

		return Error.Ok;
	}

	void GodotRequestCompleted(GodotVersion.VersionChannel channel, long result, long responseCode, string[] headers, byte[] body)
	{
		if (result != (long)HttpRequest.Result.Success)
		{
			GD.PushError("Failed to Request Godot Releases, Result Code: ", result);
			return;
		}
		GD.Print(responseCode, " OK");
		//GD.Print(headers);

		Json json = new();
		if (json.Parse(body.GetStringFromUtf8()) != Error.Ok)
		{
			GD.PushError("Failed to Parse GitHub Api Data");
		}

		/*
		We should get:
		tag_name,
		assets [
			{
				name
				content_type
				browser_download_url
			}
		]
		*/
		Array<DownloadableVersion> downloadableVersions = new();

		foreach (Dictionary version in ((Godot.Collections.Array)json.Data).Select(v => (Dictionary)v))
		{
			string versionWithoutStable = ((string)version["tag_name"]).TrimSuffix("-stable");
			SemVersion semVersion = SemVersion.Parse(versionWithoutStable, SemVersionStyles.Any);

			if (semVersion.ComparePrecedenceTo(new SemVersion(3)) == -1) // Ignoring Versions Before 3
				continue;

			//GD.Print("processing ", semVersion);
			//GD.Print("is pre-release? ", semVersion.IsPrerelease);

			// TODO: Fit for Unstable Version
			DownloadableVersion downloadableVersion = new DownloadableVersion(
				semVersion,
				channel
			);

			Godot.Collections.Array assets = (Godot.Collections.Array)version["assets"];
			foreach (Dictionary asset in assets.Select(v => (Dictionary)v))
			{
				string contentType = (string)asset["content_type"];
				string assetName = (string)asset["name"];
				string url = (string)asset["browser_download_url"];

				if (contentType == "application/zip") {
					if (assetName.Contains("macos.universal") || assetName.Contains("osx") || 	// Ignoring MacOS
						assetName.Contains("linux") || assetName.Contains("x11") || 			// Ignoring Linux
						assetName.Contains("web_editor"))										// Ignoring Web
						
						continue;
					else if (assetName.Contains("mono_win32"))
						downloadableVersion.SetDownloadUrl(DownloadableVersion.TargetPlatform.Win32Mono, url);
					else if (assetName.Contains("mono_win64"))									// Must be first
						downloadableVersion.SetDownloadUrl(DownloadableVersion.TargetPlatform.Win64Mono, url);
					else if (assetName.Contains("win32"))
						downloadableVersion.SetDownloadUrl(DownloadableVersion.TargetPlatform.Win32, url);
					else if (assetName.Contains("win64"))
						downloadableVersion.SetDownloadUrl(DownloadableVersion.TargetPlatform.Win64, url);
				} else if (contentType == "application/octet-stream")
				{
					if (assetName.Contains("tar.xz.sha256") ||									// Ignoring Godot Source
						assetName.Contains("godot-lib"))										// Ignoring Godot Libs (for Android)
						continue;
					else if (assetName.Contains("mono_export_templates"))						// Must be first
						downloadableVersion.SetExportTemplateDownloadUrl(DownloadableVersion.TargetPlatform.MonoExportTemplate, url);
					else if (assetName.Contains("export_templates"))
						downloadableVersion.SetExportTemplateDownloadUrl(DownloadableVersion.TargetPlatform.ExportTemplate, url);
				}
				else if (contentType == "application/x-xz") 									// Ignoring Godot Source
					continue;
				else if (contentType == "application/vnd.android.package-archive") 				// Ignoring Android
					continue;
				else
					continue;

				// GD.Print("name: ", asset["name"]);
			}
			downloadableVersions.Add(downloadableVersion);
		}

		EmitSignal(SignalName.RequestCompleted, downloadableVersions, (int)channel);
	}
}