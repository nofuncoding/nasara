using Godot;
using System;

namespace Nasara.Core.Management.Editor;

// TODO: Refactor to `Installer`
public partial class Downloader : Node
{
    const string CACHE_PATH = "user://godot.cache";
    Network.Github.Downloader downloader;

    [Signal]
    public delegate void DownloadFinishedEventHandler(string savePath);

    public void Download(DownloadableVersion version, bool mono)
    {
        string url = GetDownloadUrl(version, mono);

        downloader = new(url, CACHE_PATH);
        downloader.DownloadCompleted += () => {
            EmitSignal(SignalName.DownloadFinished, CACHE_PATH);
            QueueFree();
        };
        AddChild(downloader);
    }

    public string GetDownloadUrl(DownloadableVersion version, bool mono)
    {
        return version.GetDownloadUrl(GetTargetPlatform(mono));
    }

    public int GetBodySize()
    {
        if (downloader is not null)
            return downloader.GetBodySize();
        else
            return -1;
    }

    public int GetDownloadedBytes()
    {
        if (downloader is not null)
            return downloader.GetDownloadedBytes();
        else
            return -1;
    }

    public int GetSpeedPerSecond()
    {
        if (downloader is not null)
            return downloader.speedPerSecond;
        else
            return -1;
    }
    
    DownloadableVersion.TargetPlatform GetTargetPlatform(bool mono)
    {
        DownloadableVersion.TargetPlatform platform = DownloadableVersion.TargetPlatform.Win32;

        // FIXME: May got problems in build of x86 running on x86_64 pc (NEED TEST)
        // 64-bit
        if (OS.HasFeature("x86_64"))
            if (OS.HasFeature("windows"))
                if (mono)
                    platform = DownloadableVersion.TargetPlatform.Win64Mono;
                else
                    platform = DownloadableVersion.TargetPlatform.Win64;
            else if (OS.HasFeature("linux")) // Not Supported Yet
                GD.PushError("Does Not Support Linux Yet");
        // 32-bit
        else if (OS.HasFeature("x86_32"))
            if (OS.HasFeature("windows"))
                if (mono)
                    platform = DownloadableVersion.TargetPlatform.Win32Mono;
                else
                    platform = DownloadableVersion.TargetPlatform.Win32;
            else if (OS.HasFeature("linux")) // Not Supported Yet
                GD.PushError("Does Not Support Linux Yet");
        
        return platform;
    }
}