using Godot;
using System;

namespace Nasara.Network
{
    public partial class Downloader : HttpRequest
    {
        protected string Url;

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

        public override void _Ready()
        {
            GD.Print($"DOWNLOAD {Url}");
            Request(Url);
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
}