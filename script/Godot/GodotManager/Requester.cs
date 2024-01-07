using Godot;
using Godot.Collections;
using Semver;
using System;
using System.Linq;
using Nasara.Network;

namespace Nasara.GodotManager {
	public partial class Requester : Node
	{
		// GitHub
		const string GODOT_OWNER = "godotengine";
		const string GODOT_REPO_STABLE = "godot";
		const string GODOT_REPO_UNSTABLE = "godot-builds";

		[Signal]
		public delegate void VersionsRequestedEventHandler(Array<DownloadableVersion> downloadableVersions, int channel);
		
		[Signal]
		public delegate void NodeIdRequestedEventHandler(string nodeId, int channel);

		public Error RequestEditorList(GodotVersion.VersionChannel channel = GodotVersion.VersionChannel.Stable)
		{
			Network.Github.Requester github = null;
			switch (channel)
			{
				case GodotVersion.VersionChannel.Stable:
					github = new(GODOT_OWNER, GODOT_REPO_STABLE); break;
				case GodotVersion.VersionChannel.Unstable:
					github = new(GODOT_OWNER, GODOT_REPO_UNSTABLE); break;
			}
			if (github is not null)
				AddChild(github);

			if (!IsNodeReady())
				return Error.Busy;

			github.GithubRequestCompleted += (Variant result, Network.Github.Requester.RequestType type) => {
				if (type != Network.Github.Requester.RequestType.Releases)
					return;
					
				Godot.Collections.Array godots = (Godot.Collections.Array)result;
				if (godots.Count == 0)
					return;
				
				GodotRequestCompleted(channel, godots);
			};

			return Error.Ok;
		}

		void GodotRequestCompleted(GodotVersion.VersionChannel channel, Godot.Collections.Array data)
		{
			Array<DownloadableVersion> downloadableVersions = ProcessRawData(data, channel);
			if (downloadableVersions is null)
			{
				GD.PushError("(requester) Failed to process data");
				return;
			}
			EmitSignal(SignalName.VersionsRequested, downloadableVersions, (int)channel);

			Error error = SaveCache(data, channel);
			if (error != Error.Ok)
				GD.PushError("(requester) Can not save cache: ", error);
		}

		Error SaveCache(Godot.Collections.Array data, GodotVersion.VersionChannel channel)
		{
			/*
			{
				"stable": [ ... ],
				"unstable": [ ... ],
			}
			*/
			if (FileAccess.FileExists(VersionList.GODOT_LIST_CACHE_PATH))
			{
				// Processing Exists
				using var file = FileAccess.Open(VersionList.GODOT_LIST_CACHE_PATH, FileAccess.ModeFlags.ReadWrite);
				if (file is null)
					return FileAccess.GetOpenError();
				
				// Get storaged json data
				Json fileJson = new();
				if (fileJson.Parse(file.GetAsText()) != Error.Ok)
					return Error.ParseError;
				
				Dictionary version_dict = (Dictionary)fileJson.Data;

				switch (channel) {
					case GodotVersion.VersionChannel.Stable:
						version_dict["stable"] = data;
						break;
					case GodotVersion.VersionChannel.Unstable:
						version_dict["unstable"] = data;
						break;
				}
				
				file.StoreString(Json.Stringify(version_dict));
			} else {
				// Processing NotExists
				using var file = FileAccess.Open(VersionList.GODOT_LIST_CACHE_PATH, FileAccess.ModeFlags.Write);
				if (file is null)
					return FileAccess.GetOpenError();

				Dictionary version_dict = new();

				switch (channel) {
					case GodotVersion.VersionChannel.Stable:
						version_dict["stable"] = data;
						break;
					case GodotVersion.VersionChannel.Unstable:
						version_dict["unstable"] = data;
						break;
				}

				file.StoreString(Json.Stringify(version_dict));
			}

			GD.Print($"(requester) Saved Downloadable Godots Cache to {VersionList.GODOT_LIST_CACHE_PATH}");
			return Error.Ok;
		}

		public Array<DownloadableVersion> ProcessRawData(Godot.Collections.Array godots, GodotVersion.VersionChannel channel)
		{
			Array<DownloadableVersion> downloadableVersions = new();

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

			foreach (Dictionary version in godots.Select(v => (Dictionary)v))
			{
				string versionWithoutStable = ((string)version["tag_name"]).TrimSuffix("-stable");
				SemVersion semVersion = SemVersion.Parse(versionWithoutStable, SemVersionStyles.Any);

				if (semVersion.ComparePrecedenceTo(new SemVersion(3)) == -1) // Ignoring Versions Before 3
					continue;

				DownloadableVersion downloadableVersion = new(
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
				}
				downloadableVersions.Add(downloadableVersion);
			}

			return downloadableVersions;
		}

		public Error RequestLatestNodeId(GodotVersion.VersionChannel channel = GodotVersion.VersionChannel.Stable)
		{
			Network.Github.Requester github = null;
			switch (channel)
			{
				case GodotVersion.VersionChannel.Stable:
					github = new(GODOT_OWNER, GODOT_REPO_STABLE, Network.Github.Requester.RequestType.LatestNodeId); break;
				case GodotVersion.VersionChannel.Unstable:
					github = new(GODOT_OWNER, GODOT_REPO_UNSTABLE, Network.Github.Requester.RequestType.LatestNodeId); break;
			}
			if (github is not null)
				AddChild(github);
			if (!IsNodeReady())
				return Error.Busy;

			github.GithubRequestCompleted += (Variant result, Network.Github.Requester.RequestType type) => {
				string node_id = (string)result;
				EmitSignal(SignalName.NodeIdRequested, node_id, (int)channel);
			};

			return Error.Ok;
		}
	}
}