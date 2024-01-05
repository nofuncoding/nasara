using Godot;
using System;

namespace Nasara.GodotManager {
    public partial class VersionList : Node
    {
        public static readonly string GODOT_LIST_CACHE_PATH = "user://cache/remote_godot.json";
        string GodotCurrentNodeId = null;
	    string GodotUnstableCurrentNodeId = null;

        Godot.Collections.Array<DownloadableVersion> stableVersions;
        Godot.Collections.Array<DownloadableVersion> unstableVersions;

        [Signal]
        public delegate void GetListEventHandler(Godot.Collections.Dictionary<string, Godot.Collections.Array<DownloadableVersion>> list);

        // FIXME: Buggy Cache Updating
        public Error GetGodotList(Manager godotManager)
        {
            Requester godotRequester = godotManager.Requester();

            if (!FileAccess.FileExists(GODOT_LIST_CACHE_PATH)) {
                godotRequester.RequestEditorList();
                godotRequester.RequestEditorList(GodotVersion.VersionChannel.Unstable);

                return Error.Ok;
            }

            godotRequester.RequestLatestNodeId();
            godotRequester.RequestLatestNodeId(GodotVersion.VersionChannel.Unstable);

            godotRequester.NodeIdRequested += (string nodeId, int channel) => { // Updating Cache
                switch (channel)
                {
                    case (int)GodotVersion.VersionChannel.Stable:
                        if (GodotCurrentNodeId != nodeId && nodeId is not null)
                        {
                            GD.Print("(godot_list) Updating Cache for Godot Stable");
                            godotRequester.RequestEditorList();
                        }
                        break;
                    case (int)GodotVersion.VersionChannel.Unstable:
                        if (GodotUnstableCurrentNodeId != nodeId && nodeId is not null)
                        {
                            GD.Print("(godot_list) Updating Cache for Godot Unstable");
                            godotRequester.RequestEditorList(GodotVersion.VersionChannel.Unstable);
                        }
                        break;
                }
            };

            return ProcessGodotListCache(godotManager);
        }
        
        Error ProcessGodotListCache(Manager godotManager)
        {
            Requester godotRequester = godotManager.Requester();

            godotRequester.VersionsRequested += (Godot.Collections.Array<DownloadableVersion> downloadableVersions, int channel) =>
            {
                switch (channel)
                {
                    case (int)GodotVersion.VersionChannel.Stable:
                        stableVersions = downloadableVersions; break;
                    case (int)GodotVersion.VersionChannel.Unstable:
                        unstableVersions = downloadableVersions; break;
                }

                ReturnVersions();
            };

            using var file = FileAccess.Open(GODOT_LIST_CACHE_PATH, FileAccess.ModeFlags.ReadWrite);
            if (file is null)
                return FileAccess.GetOpenError();
            
            // Get storaged json data
            Json fileJson = new();
            if (fileJson.Parse(file.GetAsText()) != Error.Ok)
                return Error.ParseError;

            GD.Print($"(godot_list) Reading godot list from cache {GODOT_LIST_CACHE_PATH}");

            Godot.Collections.Dictionary version_dict = (Godot.Collections.Dictionary)fileJson.Data;

            if (version_dict.ContainsKey("stable"))
            {
                Godot.Collections.Dictionary latest = 
                (Godot.Collections.Dictionary)((Godot.Collections.Array)version_dict["stable"])[0];

                GodotCurrentNodeId = (string)latest["node_id"];

                string data = Json.Stringify(version_dict["stable"]);
                stableVersions = godotRequester.ProcessRawData(data, GodotVersion.VersionChannel.Stable);
            } else {
                // GodotRequester will auto save cache
                godotRequester.RequestEditorList();
            }

            if (version_dict.ContainsKey("unstable"))
            {
                Godot.Collections.Dictionary latest = 
                (Godot.Collections.Dictionary)((Godot.Collections.Array)version_dict["unstable"])[0];

                GodotUnstableCurrentNodeId = (string)latest["node_id"];

                string data = Json.Stringify(version_dict["unstable"]);
                unstableVersions = godotRequester.ProcessRawData(data, GodotVersion.VersionChannel.Unstable);
            } else {
                // GodotRequester will auto save cache
                godotRequester.RequestEditorList(GodotVersion.VersionChannel.Unstable);
            }

            ReturnVersions();
            
            return Error.Ok;
        }

        void ReturnVersions()
        {
            if (stableVersions is not null && unstableVersions is not null)
            {
                EmitSignal(SignalName.GetList, new Godot.Collections.Dictionary {{"stable", stableVersions}, {"unstable", unstableVersions}});
            }
        }
    }
}