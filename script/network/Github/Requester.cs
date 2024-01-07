using Godot;
using System;

namespace Nasara.Network.Github;

public partial class Requester : HttpRequest
{
    const string GITHUB_API_BASE = "https://api.github.com";

    string Url;
    RequestType Type;

    [Signal]
    public delegate void GithubRequestCompletedEventHandler(Variant result, RequestType type);

    public Requester(string owner, string repo, RequestType type=RequestType.Releases)
    {
        AppConfig config = new();
        if (config.EnableTLS)
            SetTlsOptions(TlsOptions.Client());

        Type = type;
        
        Url = $"{GITHUB_API_BASE}/repos/{owner}/{repo}/";
        switch (type)
        {
            case RequestType.Releases:
            case RequestType.LatestNodeId:
                Url += "releases";
                break;
        }

        RequestCompleted += ProcessData;
    }

    public override void _Ready()
    {
        GD.Print($"GET {Url}");
        // You don't need to call Request() any more.
        // It will be called automatically when the node is ready.
        // Very lazy, but it works.
        Request(Url);
    }

    void ProcessData(long result, long responseCode, string[] headers, byte[] body)
    {
        if (result != (long)Result.Success)
        {
            GD.PushError("(network) Failed to Request from GitHub, Result Code: ", result);
            return;
        }
        GD.Print($"{responseCode} OK {Url}");

        string data = body.GetStringFromUtf8();

        switch (Type)
        {
            case RequestType.Releases:
                EmitSignal(SignalName.GithubRequestCompleted, ProcessReleases(data), (int)Type);
                break;
            case RequestType.LatestNodeId:
                EmitSignal(SignalName.GithubRequestCompleted, ProcessLatestNodeId(data), (int)Type);
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
