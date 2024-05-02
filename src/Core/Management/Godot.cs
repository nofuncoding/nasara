using System.Threading.Tasks;

namespace Nasara.Core.Management;

public class GodotManager
{
    private GodotManifest _manifest;

    public async Task<GodotManifest> GetManifest()
    {
        //if ()
        var latest = await NetworkClient.GitHub.Repository.Release.GetLatest("godotengine", "godot");
        var latestBeta = await NetworkClient.GitHub.Repository.Release.GetLatest("godotengine", "godot-builds");
            
        Logger.Log($"{latest.Name}, {latest.Id}");
        Logger.Log($"{latestBeta.Name}, {latestBeta.Id}");
            
        return new();
    }
}

public struct GodotManifest
{
    
}

public struct DownloadableGodot
{
    public DownloadableGodot()
    {
        
    }
}

public struct GodotVersion
{
    private int majorVersion;
    private int minorVersion;
    private int patchVersion;
}