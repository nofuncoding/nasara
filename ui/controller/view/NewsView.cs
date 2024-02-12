using Godot;
using System;

using Nasara.Core.Network;

namespace Nasara.UI.View;

public partial class NewsView : Control
{
	[Export]
	VBoxContainer newsContainer;
	[Export]
	string newsHolderPath = "res://ui/component/news_holder.tscn";

	static PackedScene news_holder_scene;

	public override void _Ready()
	{
		news_holder_scene = (PackedScene)GD.Load(newsHolderPath);
		LoadContent();
	}
	
	async void LoadContent()
	{
		var feed = await GodotRssJson.Read();
		
		// Successfully parsed the feed
		if (feed.HasValue)
		{
			var items = feed.Value.items;
			for (int i = 0; i < items.Length; i++)
			{
				var item = items[i];
				
				var holder = news_holder_scene.Instantiate<Component.NewsHolder>();
				newsContainer.AddChild(holder);

				holder.SetTitle(item.title);
				holder.SetDescription(item.description);
				holder.SetLink(item.link);

				var date = DateTime.Parse(item.pubDate);
				var readable_date = date.ToString("g");
				holder.SetPubDate(readable_date);

				var image_path = await GodotRssJson.CacheImage(item);
				if (FileAccess.FileExists(image_path))
				{
					var image = Image.LoadFromFile(image_path);
					var texture = ImageTexture.CreateFromImage(image);

					holder.SetTexture(texture);
				}
			}

			GodotRssJson.DeleteUnusedCache(feed.Value);
		}

	}
}
