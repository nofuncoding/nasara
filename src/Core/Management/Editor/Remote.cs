using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Nasara.Core.Management.Editor;

// TODO define a new exception here

public struct GodotManifest
{
    private List<DownloadableGodot> _versions = [];

    public GodotManifest()
    {
        // Do nothing
    }
    
    public DownloadableGodot GetVersion(GodotVersion targetVersion)
    {
        foreach (var v in _versions.Where(v => v.GetVersion() == targetVersion))
            return v;
        
        throw new VersionNotFoundException($"the requested version {targetVersion} not found");
    }

    public IReadOnlyList<DownloadableGodot> GetVersionAll() => _versions;

    /// <summary>
    /// Parses data from GitHub release
    /// </summary>
    /// <param name="releases">The list which contained releases</param>
    public void ParseGitHub(IReadOnlyList<Release> releases)
    {
        foreach (var release in releases)
        {
            // ignoring 1.x and 2.x
            // not supported (currently)
            if (release.TagName.StartsWith('1') || release.TagName.StartsWith('2'))
                continue;
            
            // release name may be empty
            var version = new GodotVersion(release.TagName);
            _versions.Add(new DownloadableGodot(version, release.Assets));
        }
    }
}

public struct DownloadableGodot
{
    private GodotVersion _version;
    private Dictionary<GodotPlatform, string> _downloadUrl = [];
    private Dictionary<GodotPlatform, string> _exportTemplateUrl = [];
    private string _sumUrl;
    
    public DownloadableGodot(GodotVersion version, IReadOnlyList<ReleaseAsset> assets)
    {
        _version = version;
        _downloadUrl = ParseAssets(assets);
    }

    public GodotVersion GetVersion() => _version;

    public string GetDownloadUrl(GodotPlatform platform)
    {
        return _downloadUrl[platform];
    }
    
    public string GetExportTemplateUrl(GodotPlatform platform)
    {
        // get current language's export template url
        // it doesn't matter whatever it's on which platform
        foreach (var asset in _exportTemplateUrl.Where(asset => 
                     asset.Key.SupportedLanguage == platform.SupportedLanguage))
        {
            return asset.Value;
        }
        
        return "";
    }

    public string GetSumUrl() => _sumUrl;
    
    /// <summary>
    /// Parse GitHub assets to properties
    /// </summary>
    /// <param name="assets"></param>
    private Dictionary<GodotPlatform, string> ParseAssets(IReadOnlyList<ReleaseAsset> assets)
    {
        foreach (var asset in assets)
        {
            // Logger.Log($"Asset in {_version}: {asset.Name}({asset.Id}) {asset.BrowserDownloadUrl} > {asset.ContentType}");
            if (asset.ContentType == "application/zip")
            {
                if (asset.Name.Contains("macos.universal") || asset.Name.Contains("osx") || 	// Ignoring MacOS
                    asset.Name.Contains("linux") || asset.Name.Contains("x11") || 			    // Ignoring Linux
                    asset.Name.Contains("web_editor"))										    // Ignoring Web
                    continue;

                // Windows
                if (asset.Name.Contains("win32"))
                {
                    var platform = new GodotPlatform()
                    {
                        CurrentPlatform = GodotPlatform.Platform.Windows,
                        SupportedLanguage = asset.Name.Contains("mono") ? GodotPlatform.Language.Mono : GodotPlatform.Language.GDScript,
                        PlatformArchitecture = GodotPlatform.Architecture.Bit32,
                    };

                    _downloadUrl.Add(platform, asset.BrowserDownloadUrl);
                }
                else if (asset.Name.Contains("win64"))
                {
                    var platform = new GodotPlatform()
                    {
                        CurrentPlatform = GodotPlatform.Platform.Windows,
                        SupportedLanguage = asset.Name.Contains("mono") ? GodotPlatform.Language.Mono : GodotPlatform.Language.GDScript,
                        PlatformArchitecture = GodotPlatform.Architecture.Bit64,
                    };

                    _downloadUrl.Add(platform, asset.BrowserDownloadUrl);
                }
            }
            else if (asset.ContentType == "application/octet-stream")
            {
                if (asset.Name.Contains("tar.xz.sha256") ||									// Ignoring Godot Source
                    asset.Name.Contains("godot-lib"))										// Ignoring Godot Libs (for Android)
                    continue;

                if (asset.Name.Contains("export_templates"))
                {
                    var platform = new GodotPlatform()
                    {
                        SupportedLanguage = asset.Name.Contains("mono") ? GodotPlatform.Language.Mono : GodotPlatform.Language.GDScript,
                    };
                    
                    _exportTemplateUrl.Add(platform, asset.BrowserDownloadUrl);   
                }
            }
            else if (asset.ContentType.Contains("text/plain"))                                  // SHA512-SUM
                _sumUrl = asset.BrowserDownloadUrl;
            else if (asset.ContentType == "application/x-xz") 									// Ignoring Godot Source
                continue;
            else if (asset.ContentType == "application/vnd.android.package-archive") 			// Ignoring Android
                continue;
            else
                continue;
        }

        return new();
    }
}

internal class VersionNotFoundException : Exception
{
    public VersionNotFoundException()
    {
        
    }

    public VersionNotFoundException(string message)
        : base(message)
    {
        
    }

    public VersionNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
        
    }
}