using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nasara.Core.Management.Editor;

public class GodotManager
{
    public async Task<GodotManifest> GetManifest()
    {
        //if ()
        var latest = await NetworkClient.GitHub.Repository.Release.GetLatest("godotengine", "godot");
        var latestBeta = await NetworkClient.GitHub.Repository.Release.GetLatest("godotengine", "godot-builds");
            
        Logger.Log($"{latest.Name}, {latest.Id}");
        Logger.Log($"{latestBeta.Name}, {latestBeta.Id}");

        var v0 = new GodotVersion("4.0-dev1");
        var v1 = new GodotVersion("v4.0-dev1");
        var v2 = new GodotVersion("v4.0.1-stable");
        var v3 = new GodotVersion("3.0.1-stable");
        
        Logger.Log($"{v0}, {v1}, {v2}, {v3}");
            
        return new();
    }
}

public struct GodotVersion
{
    private int _majorVersion;
    private int _minorVersion;
    private int _patchVersion;
    private Channel _channel;
    /// <summary>
    /// If the channel is stable, it will be 0.
    /// </summary>
    private int _prereleaseVersion;
    
    public GodotVersion(string versionString)
    {
        Logger.Log($"Parsing {versionString}");
        // if (!versionString.StartsWith('v')) throw new FormatException("should start with 'v'");
        
        // Turn the raw string into this:
        // ["4", "0", "1", "-stable"] or
        // ["4", "0", "-rc1"]
        var splited = versionString.Trim('v').Split('.');
        if (splited.Length == 3) // versions like "4.0-stable"
        {
            splited[3] = splited[2]; // ["4", "0", "-rc1", "-rc1"]
            splited[2] = "0"; // ["4", "0", "0", "-rc1"]
        }
        var prerelease = splited[-1].Split('-', 1);
        splited = splited.SkipLast(1).ToArray();
        splited = [.. prerelease];

        var version = new GodotVersion();
        version._majorVersion = int.Parse(splited[0]);
        version._minorVersion = int.Parse(splited[1]);
        version._patchVersion = int.Parse(splited[2]);
    }

    public override string ToString()
    {
        var str = $"{_majorVersion}.{_minorVersion}";
        if (_patchVersion != 0)
            str += $".{_patchVersion}";

        str += _channel switch
        {
            Channel.Dev => $"-dev{_prereleaseVersion}",
            Channel.Beta => $"-beta{_prereleaseVersion}",
            Channel.Rc => $"-rc{_prereleaseVersion}",
            Channel.Stable => $"-stable",
            _ => "-unknown",
        };

        return str;
    }

    public override bool Equals(object obj)
    {
        if (obj is GodotVersion version)
            return this == version;
        return false;
    }

    public override int GetHashCode() =>
        HashCode.Combine(_majorVersion, _minorVersion, _patchVersion, _channel, _prereleaseVersion);

    public static bool operator ==(GodotVersion left, GodotVersion right) =>
        left._majorVersion == right._majorVersion &&
        left._minorVersion == right._minorVersion &&
        left._patchVersion == right._patchVersion &&
        left._channel == right._channel &&
        left._prereleaseVersion == right._prereleaseVersion;

    public static bool operator !=(GodotVersion left, GodotVersion right) => !(left == right);

    public static bool operator >(GodotVersion left, GodotVersion right)
    {
        if (left._majorVersion == right._majorVersion)
            if (left._minorVersion == right._minorVersion)
                if (left._patchVersion == right._patchVersion)
                    if (left._channel == right._channel)
                        return left._prereleaseVersion > right._prereleaseVersion;
                    else
                        return left._channel > right._channel;
                else
                    return left._patchVersion > right._patchVersion;
            else
                return left._minorVersion > right._minorVersion;
        
        return left._majorVersion > right._majorVersion;
    }

    public static bool operator <(GodotVersion left, GodotVersion right) => !(left > right);
    
    public enum Channel
    {
        Dev,
        Beta,
        Rc,
        Stable,
    }
}