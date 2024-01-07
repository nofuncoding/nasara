using Godot;
using System;

namespace Nasara.Network;

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

    public override void _Process(double delta)
    {
        // Update Speed
        int downloadedBytes = GetDownloadedBytes();
        int speed = (int)Math.Floor((downloadedBytes - prevDownloadedBytes) / delta);
        if (speed > 0) // the speed may become 0 when the update is too fast
            speedPerSecond = speed; // if the speed is 0, it will not be changed.
        prevDownloadedBytes = downloadedBytes;
    }

    public override void _Ready()
    {
        GD.Print($"DOWNLOAD {Url}");
        Request(Url);
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