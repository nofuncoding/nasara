using Godot;
using System;

namespace Nasara.Core.Network;

public partial class Downloader : HttpRequest
{
    public int speedPerSecond = -1;
    protected string Url;

    int prevDownloadedBytes = 0;

    [Signal]
    public delegate void DownloadCompletedEventHandler();

    public Downloader(string url, string save_path)
    {
        Url = url;
        UseThreads = true;
        DownloadFile = save_path;

        AppConfig config = new();
        if (config.EnableTLS)
            SetTlsOptions(TlsOptions.Client());
        
        RequestCompleted += CompleteDownloading;
    }

    void UpdateSpeed()
    {
        int downloadedBytes = GetDownloadedBytes();
        int speed = (int)Math.Floor((downloadedBytes - prevDownloadedBytes) / 0.5f);
        speedPerSecond = speed;
        prevDownloadedBytes = downloadedBytes;

        // GD.Print($"Total Downloaded: {downloadedBytes} bytes, Previous Downloaded: {prevDownloadedBytes} bytes, Downloaded: {downloadedBytes} bytes,\nSpeed: {speedPerSecond} bytes/s");
    }

    public override void _Ready()
    {
        GD.Print($"DOWNLOAD {Url}");
        Request(Url);

        // Add a Timer to update speed
        Timer timer = new()
        {
            OneShot = false,
            WaitTime = 0.5f
        };
        timer.Timeout += UpdateSpeed;

        AddChild(timer);
        timer.Start();
    }

    public override void _Notification(int what)
    {
        switch (what)
        {
            case (int)NotificationWMCloseRequest:
                Cancel();
                break;
        }
    }

    public void Cancel()
    {
        GD.Print("DOWNLOAD CANCELLED");
        CancelRequest();
        QueueFree();
    }

    void CompleteDownloading(long result, long responseCode, string[] headers, byte[] body)
    {
        if (result != (long)Result.Success)
        {
            GD.PushError("(network) Failed to Download the File, Result Code: ", result);
            return;
        }
        
        GD.Print("DOWNLOAD COMPLETED");
        EmitSignal(SignalName.DownloadCompleted);

        QueueFree();
    }
}