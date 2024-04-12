using System.Net.Http;
using Octokit;

namespace Nasara.Core;

public class NetworkClient
{
    private static NetworkClient _instance;
    private static GitHubClient _gitHubClient;
    private static HttpClient _httpClient;

    private NetworkClient()
    {
        // initialize backends
        _gitHubClient = new GitHubClient(new ProductHeaderValue("nasara"));
        _httpClient = new HttpClient();
    }
    
    public static void Initialize()
    {
        if (_instance is not null) return;
        
        App.Log("Initializing", "NetworkClient");
        _instance = new NetworkClient();
    }

    public static NetworkClient Get() => _instance;
}