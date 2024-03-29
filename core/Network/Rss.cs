using Godot;
using System;
using System.Web;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nasara.Core.Network;

// This class is only used to parse the Godot Engine Official RSS feed.

public class GodotRssJson
{
    const string RSS_URL = "https://godotengine.org/rss.json";
    static readonly string IMG_CACHE_PATH = App.CACHE_PATH.PathJoin("rss");

    public static async Task<GodotRssFeed?> Read()
    {
        try
        {
            // Get the json string
            GD.Print($"GET {RSS_URL}");
            using var response = await App.sysHttpClient.GetAsync(RSS_URL);
            var code = response.EnsureSuccessStatusCode();
            GD.Print($"{code.StatusCode} {RSS_URL}");
            string responseBody = await response.Content.ReadAsStringAsync();

            // Parse the json string
            Json json = new();
            if (json.Parse(responseBody) != Error.Ok)
            { 
                GD.PushError($"Error parsing RSS JSON: (line {json.GetErrorLine()}) {json.GetErrorMessage()}");
            }

            // Convert the json data to a GodotRssFeed object
            var data = (Godot.Collections.Dictionary)json.Data;
            GodotRssFeed feed = new(data);

            return feed;
        }
        catch (HttpRequestException e)
        {
            GD.PushError($"{e.Message} ERR {RSS_URL}");
        }
        return null;
    }

    public static async Task<string[]> CacheImages(GodotRssFeed feed)
    {
        DirAccess.MakeDirAbsolute(IMG_CACHE_PATH);

        var image_path = new string[feed.items.Length];
        for (int i = 0; i < feed.items.Length; i++)
        {
            Uri uri = new(feed.items[i].image);
            
            try
            {
                var filename = uri.Segments.Last();
                var save_path = IMG_CACHE_PATH.PathJoin(filename);

                // The cache already exists, so we can skip the download
                if (FileAccess.FileExists(save_path))
                {
                    image_path[i] = save_path;
                    continue;
                }

                // Download the image
                GD.Print($"CACHE {uri}");
                using HttpResponseMessage response = await App.sysHttpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                byte[] body = await response.Content.ReadAsByteArrayAsync();

                // Save it
                using var file = FileAccess.Open(save_path, FileAccess.ModeFlags.Write);
                if (file is null)
                {
                    GD.PushError($"Failed to save cache to {save_path}");
                    image_path[i] = "";
                    continue;
                }
                file.StoreBuffer(body);

                image_path[i] = save_path;
            }
            catch (HttpRequestException e)
            {
                GD.PushError($"{e.Message} ERR {uri}");
            }
        }
        
        return [];
    }

    public static async Task<string> CacheImage(GodotRssItem item)
    {
        Uri uri = new(item.image);

        DirAccess.MakeDirAbsolute(IMG_CACHE_PATH);

        try
        {
            var filename = uri.Segments.Last();
            var save_path = IMG_CACHE_PATH.PathJoin(filename);

            // The cache already exists
            if (FileAccess.FileExists(save_path))
            {
                /*using var rfile = FileAccess.Open(save_path, FileAccess.ModeFlags.Read);
                // TODO: Use `HEAD` and Update Time instead of downloading the whole image
                if (rfile is not null && rfile.GetBuffer((long)rfile.GetLength()) == body)*/
                    return save_path;
            }

            // Download the image
            GD.Print($"CACHE {uri}");
            using HttpResponseMessage response = await App.sysHttpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            byte[] body = await response.Content.ReadAsByteArrayAsync();

            // Save it
            using var file = FileAccess.Open(save_path, FileAccess.ModeFlags.Write);
            if (file is null)
            {
                GD.PushError($"Failed to save cache to {save_path}");
                return "";
            }
            file.StoreBuffer(body);

            return save_path;
        }
        catch (HttpRequestException e)
        {
            GD.PushError($"{e.Message} ERR {uri}");
        }

        return "";
    }

    public static Error DeleteUnusedCache(GodotRssFeed feed)
    {
        using var dir = DirAccess.Open(IMG_CACHE_PATH);
        if (dir is null)
        {
            var err = DirAccess.GetOpenError();
            GD.PushError($"Failed to open cache directory {IMG_CACHE_PATH}: {err}");
            return err;
        }
        var delete_list = dir.GetFiles().ToList();

        foreach (var item in feed.items)
        {
            Uri uri = new(item.image);
            var filename = uri.Segments.Last();
            delete_list.Remove(filename);
        }

        if (delete_list.Count > 0)
        {
            foreach (var filename in delete_list)
            {
                var f = IMG_CACHE_PATH.PathJoin(filename);
                GD.Print($"DELETE {f}");
                DirAccess.RemoveAbsolute(f);
            }
        }

        return Error.Ok;
    }
}

public readonly struct GodotRssFeed
{
    public GodotRssFeed(Godot.Collections.Dictionary data)
    {
        title = HttpUtility.HtmlDecode((string)data["title"]);
        description = HttpUtility.HtmlDecode((string)data["description"]);

        var item_list = (Godot.Collections.Array)data["items"];

        items = new GodotRssItem[item_list.Count];
        for (int i = 0; i < item_list.Count; i++)
        {
            GodotRssItem item = new((Godot.Collections.Dictionary)item_list[i]);
            items[i] = item;
        }
    }

    public readonly string title;
    public readonly string description;
    public readonly GodotRssItem[] items;
}

public readonly struct GodotRssItem(Godot.Collections.Dictionary data)
{
    public readonly string title = HttpUtility.HtmlDecode((string)data["title"]);
    public readonly string description = HttpUtility.HtmlDecode((string)data["description"]);
    public readonly string link = (string)data["link"];
    public readonly string pubDate = (string)data["pubDate"];
    public readonly string image = (string)data["image"];
    public readonly string creator = (string)data["dc:creator"];
}