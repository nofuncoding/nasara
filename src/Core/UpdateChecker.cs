using System;
using System.Linq;

namespace Nasara.Core;

public class UpdateChecker
{
    public static async void CheckUpdate(bool beta=false)
    {
        if (!NetworkClient.IsInitiaized())
        {
            Logger.LogWarn("NetworkClient is not initialized.", "UpdateChecker");
            return;
        }
        
        Logger.Log("Checking updates", "UpdateChecker");

        var currentVersionString = App.GetVersion();
        var currentVersion = currentVersionString.Trim('v').Split('.').Select(int.Parse).ToArray();
        
        
        try
        {
            // This will NOT get the latest pre-release version.
            var release = await NetworkClient.GitHub.Repository.Release.GetLatest("nofuncoding", "nasara");
            var latestVersion = release.Name.Trim('v').Split('.').Select(int.Parse).ToArray();
            
            // Major version
            if (latestVersion[0] > currentVersion[0])
            {
                
            }
            else
            {
                // Minor version
                if (latestVersion[1] > currentVersion[1])
                {
                    
                }
                else
                {
                    // Patch version
                    if (latestVersion[2] > currentVersion[2])
                    {
                        
                    }
                    // No updates
                    else
                    {
                        Logger.Log("Up-to-date", "UpdateChecker");
                    }
                }
            }
        }
        catch
        {
            Logger.LogWarn("Failed to get updates", "UpdateChecker");
        }
        
    }
}