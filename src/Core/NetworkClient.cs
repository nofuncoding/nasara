using System;
using Godot;
using Octokit;
using HttpClient = System.Net.Http.HttpClient;

namespace Nasara.Core;

// TODO The anything about networking should be processed here,
//      including error processing.

public static class NetworkClient
{
    private static bool _initialized = false;
    
    public static GitHubClient GitHub { get; private set; }
    public static HttpClient Http { get; private set; }
    
    public static void Initialize()
    {
        if (_initialized) return;
        
        Logger.Log("Initializing");
        // initialize backends
        GitHub = new GitHubClient(new ProductHeaderValue("nasara"));
        Http = new HttpClient();

        _initialized = true;
    }

    public static bool IsInitiaized() => _initialized;
}