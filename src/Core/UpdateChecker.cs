using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nasara.Core;

public static class UpdateChecker
{
    public static async Task CheckUpdate(bool beta=false)
    {
        if (!NetworkClient.IsInitiaized())
        {
            Logger.LogWarn("NetworkClient is not initialized.");
            return;
        }
        
        Logger.Log("Checking updates");

        var currentVersionString = App.GetVersion();
        var currentVersion = currentVersionString.Trim('v').Split('.').Select(int.Parse).ToArray();
        
        try
        {
            // This will NOT get the latest pre-release version.
            var release = await NetworkClient.GitHub.Repository.Release.GetLatest("nofuncoding", "nasara");
            // this may cause problem in "v1.0.0-rc.1"
            // but since GitHub will not set a prerelease version as latest version,
            // it's work RIGHT NOW. so, TODO.
            var latestVersion = release.Name.Trim('v').Split('.').Select(int.Parse).ToArray();
            
            // Major version
            if (latestVersion[0] > currentVersion[0])
            {
                Logger.Log("A major version is available");
                return;
            }
            
            if (latestVersion[0] == currentVersion[0])
            {
                // Minor version
                if (latestVersion[1] > currentVersion[1])
                {
                    Logger.Log("A minor version is available");
                    return;
                }
                
                if (latestVersion[1] == currentVersion[1])
                {
                    // Patch version
                    if (latestVersion[2] > currentVersion[2])
                    {
                        Logger.Log("A patch version is available");
                        return;
                    }
                }
            }
            
            // No update
            Logger.Log("Up-to-date");
        }
        catch
        {
            Logger.LogWarn("Failed to get updates");
        }
    }
}