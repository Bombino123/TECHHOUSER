using System;

namespace GMap.NET.MapProviders;

public class OpenStreetMapQuestSatelliteProvider : OpenStreetMapProviderBase
{
	public static readonly OpenStreetMapQuestSatelliteProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("E590D3B1-37F4-442B-9395-ADB035627F67");


	public override string Name { get; } = "OpenStreetMapQuestSatellite";


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

	private OpenStreetMapQuestSatelliteProvider()
	{
		Copyright = $"© MapQuest - Map data ©{DateTime.Today.Year} MapQuest, OpenStreetMap";
	}

	static OpenStreetMapQuestSatelliteProvider()
	{
		UrlFormat = "http://otile{0}.mqcdn.com/tiles/1.0.0/sat/{1}/{2}/{3}.jpg";
		Instance = new OpenStreetMapQuestSatelliteProvider();
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
