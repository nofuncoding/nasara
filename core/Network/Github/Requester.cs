using Godot;
using System;
using System.Threading.Tasks;
using Http = System.Net.Http;

namespace Nasara.Core.Network.Github;

public static class Requester
{
    const string GITHUB_API_BASE = "https://api.github.com/";
    static Http.HttpClient _client;

    public static void Init()
    {
        _client = new()
        {
            BaseAddress = new Uri(GITHUB_API_BASE),
            Timeout = TimeSpan.FromSeconds(10),
        };
        _client.DefaultRequestHeaders.Add("User-Agent", $"Nasara/{ProjectSettings.GetSetting("application/config/version")}");
    }

    public static async Task<Godot.Collections.Array> RequestReleases(string owner, string repo)
    {
        using var response = await _client.GetAsync($"repos/{owner}/{repo}/releases");
        GD.Print($"GET {response.RequestMessage.RequestUri}");
        var code = response.EnsureSuccessStatusCode();
        
        string responseBody = await response.Content.ReadAsStringAsync();

        return ProcessReleases(responseBody);
    }

    public static async Task<string> RequestLatestNodeId(string owner, string repo)
    {
        using var response = await _client.GetAsync($"repos/{owner}/{repo}/releases");
        var code = response.EnsureSuccessStatusCode();

        string responseBody = response.Content.ReadAsStringAsync().Result;

        return ProcessLatestNodeId(responseBody);
    }

    static Godot.Collections.Array ProcessReleases(string data)
    {
        Json json = new();
        if (json.Parse(data) != Error.Ok)
        {
            GD.PushError($"Failed to Parse GitHub API Data: (line {json.GetErrorLine()}) {json.GetErrorMessage()}");
            return null;
        }

        return (Godot.Collections.Array)json.Data;
    }

    static string ProcessLatestNodeId(string data)
    {
        Godot.Collections.Array releases = ProcessReleases(data);
        if (releases is null || releases.Count == 0)
            return "";

        Godot.Collections.Dictionary latestRelease = (Godot.Collections.Dictionary)releases[0];

        return (string)latestRelease["node_id"];
    }
}
