using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nasara.Core.Management.Editor;

public class GodotManager
{
    private static GodotManifest? _cachedManifest;
    
    /// <summary>
    /// Request a new <see cref="GodotManifest"/> that contains
    /// all versions in the specified channel
    /// </summary>
    /// <param name="release">If true, only request release versions (from godotengine/godot)</param>
    /// <param name="refresh">Refresh the cached manifest</param>
    /// <returns></returns>
    public async Task<GodotManifest> GetManifest(bool release = true, bool refresh = false)
    {
        Logger.Log("Requesting version manifest");

        if (!refresh && _cachedManifest is not null)
            return _cachedManifest.GetValueOrDefault(new()); // Okay, wtf is this?
        
        var manifest = new GodotManifest();

        try
        {
            if (release)
                manifest.ParseGitHub(await NetworkClient.GitHub.Repository.Release.GetAll("godotengine", "godot"));
            else
                manifest.ParseGitHub(
                    await NetworkClient.GitHub.Repository.Release.GetAll("godotengine", "godot-builds"));

            _cachedManifest = manifest;
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to request version manifest: {e}");
        }

        return manifest;
    }
}

public struct GodotVersion
{
    private int _majorVersion = 4;
    private int _minorVersion = 0;
    private int _patchVersion = 0;
    private Channel _channel = Channel.Stable;
    /// <summary>
    /// If the channel is stable, it will be 0.
    /// </summary>
    private int _prereleaseVersion = 0;
    
    public GodotVersion(string versionString)
    {
        // Currently, this function do not support version like 2.0.4.1
        // What the hell is it?
        
        // if (!versionString.StartsWith('v')) throw new FormatException("should start with 'v'");
        
        // Turn the raw string into this:
        // ["4", "0", "1-stable"] or
        // ["4", "0-dev0"]
        var splited = versionString.Trim('v').Split('.').ToList();

        if (splited.Count == 2) // versions like "4.0-dev0"
        {
            splited.Add(splited.Last());                                      // ["4", "0-dev0", "0-dev0"]
            splited[1] = "0";                                                     // ["4", "0", "0-dev0"]
        }

        var prerelease = splited
            .Last()                                                                     // "0-dev0"
            .Split('-');                                                        // ["0", "dev0"]
        
        if (prerelease.Last().Contains("dev") || prerelease.Last().Contains("beta") || prerelease.Last().Contains("rc"))
        {
            var rawPrereleaseVersion = prerelease.Last();
            // TODO may broke if number's length is 2
            // (but it doesn't make any sense to fix it now)
            
            // copy string except the number
            var str = new string(rawPrereleaseVersion.SkipLast(1).ToArray());
            // the number
            var number = rawPrereleaseVersion.Last().ToString();

            prerelease = prerelease.SkipLast(1).Concat([str, number]).ToArray();        // ["0", "dev", "0"]
        }
        splited = splited.SkipLast(1).ToList();                                   // ["4", "0"]
        splited = splited.Concat(prerelease).ToList();                            // ["4", "0", "0", "dev", "0"]

        // Finally! init the version
        _majorVersion = int.Parse(splited[0]);
        _minorVersion = int.Parse(splited[1]);
        _patchVersion = int.Parse(splited[2]);
        _channel = splited[3] switch
        {
            "dev" => Channel.Dev,
            "beta" => Channel.Beta,
            "rc" => Channel.Rc,
            "stable" => Channel.Stable,
            _ => throw new Exception($"Unknown channel {splited[3]}")
        };
        if (_channel != Channel.Stable)
            _prereleaseVersion = int.Parse(splited[4]);
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
            _ => throw new Exception($"Unknown channel {_channel}")
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
        // check by-level
        if (left._majorVersion != right._majorVersion) return left._majorVersion > right._majorVersion;
        if (left._minorVersion != right._minorVersion) return left._minorVersion > right._minorVersion;
        if (left._patchVersion != right._patchVersion) return left._patchVersion > right._patchVersion;
        if (left._channel != right._channel) return left._channel > right._channel;
        return left._prereleaseVersion > right._prereleaseVersion;
    }

    public static bool operator <(GodotVersion left, GodotVersion right) => !(left > right); // well, does it work?
    
    public enum Channel
    {
        Dev,
        Beta,
        Rc,
        Stable,
    }
}

public struct GodotPlatform
{
    public Platform CurrentPlatform = Platform.Windows;
    public Language SupportedLanguage = Language.GDScript;
    public Architecture PlatformArchitecture = Architecture.Bit64;
    
    public GodotPlatform() { }
    
    public enum Platform
    {
        Windows,
        Unix,
        // No support plan for
        // OSX,
    }

    public enum Language
    {
        GDScript,
        Mono,
    }

    public enum Architecture
    {
        Bit64,
        Bit32,
    }
}