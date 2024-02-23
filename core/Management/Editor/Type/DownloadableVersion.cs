using Godot;
using System;
using Semver;
using System.Threading.Tasks;

namespace Nasara.Core.Management.Editor;

// TODO: Refactor this to request url when needed

[GlobalClass]
public partial class DownloadableVersion : RefCounted
{
    public SemVersion Version { get; }
    public GodotVersion.VersionChannel Channel { get; }

    // TODO: Support Other Platforms (e.g. MacOS, Linux(X11) )
    // TODO: Refactor these to make code clear
    // TODO: Add Sha512 Verify
    
    // Downloads
    string Win32Download;
    string Win32MonoDownload;
    string Win64Download;
    string Win64MonoDownload;
    
    string ExportTemplateDownload;
    string MonoExportTemplateDownload;

    string Sha512FileDownload;

    Godot.Collections.Dictionary<TargetPlatform, string> fileSha512;

    // (Mono)+(Platform)+(Arch)
    // Mono:        Yes     200   |   No     100
    // Platform:    Windows 000   |   Linux  010   |   Export Templates 090
    // Arch:        x86_64  000   |   x64    001
    // e.g. Windows x64 Mono = 201
    // TODO: Wtf is this? Remove it later

    public enum TargetPlatform
    {
        MonoExportTemplate = 290,
        ExportTemplate     = 190,

        Win32       = 100,
        Win64       = 101,
        Win32Mono   = 200,
        Win64Mono   = 201,

        // For the future
        Linux32       = 110,
        Linux64       = 111,
        Linux32Mono   = 210,
        Linux64Mono   = 211,
    }

    public DownloadableVersion(string version, GodotVersion.VersionChannel channel)
    {
        Version = SemVersion.Parse(version, SemVersionStyles.Strict);
        Channel = channel;
    }

    public DownloadableVersion(SemVersion version, GodotVersion.VersionChannel channel)
    {
        Version = version;
        Channel = channel;
    }

    public string GetDownloadUrl(TargetPlatform platform)
    {
        switch (platform)
        {
            case TargetPlatform.Win32:
                return Win32Download;
            case TargetPlatform.Win64:
                return Win64Download;
            case TargetPlatform.Win32Mono:
                return Win32MonoDownload;
            case TargetPlatform.Win64Mono:
                return Win64MonoDownload;
            default:
                GD.PushError($"Platform Type Not Supported ({(int)platform})");
                return null;
        }
    }

    public string GetExportTemplateDownloadUrl(TargetPlatform platform)
    {
        switch (platform)
        {
            case TargetPlatform.MonoExportTemplate:
                return MonoExportTemplateDownload;
            case TargetPlatform.ExportTemplate:
                return ExportTemplateDownload;
            default:
                GD.PushError($"Not a Export Template Type ({(int)platform})");
                return null;
        }
    }

    public async Task<string> GetSha512Async(TargetPlatform platform)
    {
        if (fileSha512 is null)
            await RequestSha512();

        return fileSha512[platform];
    }

    async Task RequestSha512()
    {
        fileSha512 = [];
        var res = await App.sysHttpClient.GetStringAsync(Sha512FileDownload);
        var lines = res.Split('\n');

        foreach (var line in lines)
        {
            // GD.Print($"line: {line}");
            var split = line.Split("  ");
            if (split.Length != 2)
                continue;

            var sha512 = split[0];

            var filename = split[1];
            var ext = filename[(filename.LastIndexOf('.') + 1)..];
            var platform = TargetPlatform.Win64;

            if (ext == "sha256" || ext == "xz" ||
                ext == "aar" || ext == "aab" || ext == "apk") // ignoring source & android
                continue;
            else if (ext == "zip")
            {
                if (filename.Contains("macos.universal") || filename.Contains("osx") || // Ignoring MacOS
                    filename.Contains("linux") || filename.Contains("x11") || 			// Ignoring Linux
                    filename.Contains("web_editor"))									// Ignoring Web
                    continue;
                else if (filename.Contains("mono_win32"))
                    platform = TargetPlatform.Win32Mono;
                else if (filename.Contains("mono_win64"))								// Must be first
                    platform = TargetPlatform.Win64Mono;
                else if (filename.Contains("win32"))
                    platform = TargetPlatform.Win32;
                else if (filename.Contains("win64"))
                    platform = TargetPlatform.Win64;
            }
            else if (ext == "tpz")
            {
                if (filename.Contains("mono_export_templates"))						// Must be first
					platform = TargetPlatform.MonoExportTemplate;
                else if (filename.Contains("export_templates"))
                    platform = TargetPlatform.ExportTemplate;
            }

            // GD.Print($"{platform}");
            fileSha512[platform] = sha512;
        }
    }

    public void SetSumUrl(string url) => Sha512FileDownload = url;

    public void SetDownloadUrl(TargetPlatform platform, string url)
    {
        switch (platform)
        {
            case TargetPlatform.Win32:
                Win32Download = url; break;
            case TargetPlatform.Win64:
                Win64Download = url; break;
            case TargetPlatform.Win32Mono:
                Win32MonoDownload = url; break;
            case TargetPlatform.Win64Mono:
                Win64MonoDownload = url; break;
        }
    }

    public void SetExportTemplateDownloadUrl(TargetPlatform platform, string url)
    {
        if ((int)platform >= 200)
            MonoExportTemplateDownload = url;
        else
            ExportTemplateDownload = url;
    }

    public void SetSha512(TargetPlatform platform, string Sha256)
    {
        fileSha512[platform] = Sha256;
    }
}
