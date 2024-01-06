using Godot;
using System;

namespace Nasara.Network.Github
{
    // TODO: Use owner, repo to get lazy releases
    public partial class Downloader : Network.Downloader
    {
        public Downloader(string url, string save_path) : base(url, save_path)
        {
            if (new AppConfig().UsingGithubProxy)
                Url = "https://mirror.ghproxy.com/" + url; // TODO: Replace it using a better way
        }
    }
}