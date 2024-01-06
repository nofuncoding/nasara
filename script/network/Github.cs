using Godot;
using System;
using System.ComponentModel;

namespace Nasara.Network
{
    public partial class Github : HttpRequest
    {
        const string GITHUB_API_BASE = "https://api.github.com";

        bool usingProxy = false;
        string url;
        RequestType type;

        [Signal]
        public delegate void GithubRequestCompletedEventHandler(Variant result, RequestType type);

        public Github(string owner, string repo, RequestType type=RequestType.Releases)
        {
            AppConfig config = new();
            if (config.EnableTLS)
                SetTlsOptions(TlsOptions.Client());
            if (config.UsingGithubProxy)
                usingProxy = true;

            this.type = type;
            
            url = $"{GITHUB_API_BASE}/repos/{owner}/{repo}/";
            switch (type)
            {
                case RequestType.Releases:
                case RequestType.LatestNodeId:
                    url += "releases";
                    break;
            }

            RequestCompleted += ProcessData;
        }

        public override void _Ready()
        {
            GD.Print($"GET {url}");
            Request(url);
        }

        void ProcessData(long result, long responseCode, string[] headers, byte[] body)
        {
            if (result != (long)Result.Success)
            {
                GD.PushError("(network) Failed to Request from GitHub, Result Code: ", result);
				return;
            }
            GD.Print($"{responseCode} OK {url}");

            string data = body.GetStringFromUtf8();

            switch (type)
            {
                case RequestType.Releases:
                    EmitSignal(SignalName.GithubRequestCompleted, ProcessReleases(data), (int)type);
                    break;
                case RequestType.LatestNodeId:
                    EmitSignal(SignalName.GithubRequestCompleted, ProcessLatestNodeId(data), (int)type);
                    break;
            }

            QueueFree();
        }

        Godot.Collections.Array ProcessReleases(string data)
        {
            Json json = new();
			if (json.Parse(data) != Error.Ok)
			{
				GD.PushError("Failed to Parse GitHub API Data");
				return null;
			}

            return (Godot.Collections.Array)json.Data;
        }

        string ProcessLatestNodeId(string data)
        {
            Godot.Collections.Array releases = ProcessReleases(data);
            if (releases is null || releases.Count == 0)
                return "";

            Godot.Collections.Dictionary latestRelease = (Godot.Collections.Dictionary)releases[0];

            return (string)latestRelease["node_id"];
        }

        public enum RequestType
        {
            Releases,
            LatestNodeId
        }
    }
}