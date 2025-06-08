using System;

namespace GMap.NET.MapProviders;

public class OpenStreetMapQuestProvider : OpenStreetMapProviderBase
{
	public static readonly OpenStreetMapQuestProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("D0A12840-973A-448B-B9C2-89B8A07DFF0F");


	public override string Name { get; } = "OpenStreetMapQuest";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[1] { this };
			}
			return _overlays;
		}
	}

	private OpenStreetMapQuestProvider()
	{
		Copyright = $"© MapQuest - Map data ©{DateTime.Today.Year} MapQuest, OpenStreetMap";
	}

	static OpenStreetMapQuestProvider()
	{
		UrlFormat = "http://otile{0}.mqcdn.com/tiles/1.0.0/osm/{1}/{2}/{3}.png";
		Instance = new OpenStreetMapQuestProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 3) + 1, zoom, pos.X, pos.Y);
	}
}
