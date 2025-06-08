using System;

namespace GMap.NET.MapProviders;

public class YahooHybridMapProvider : YahooMapProviderBase
{
	public static readonly YahooHybridMapProvider Instance;

	public string Version = "4.3";

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("A084E0DB-F9A6-45C1-BC2F-791E1F4E958E");


	public override string Name { get; } = "YahooHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					YahooSatelliteMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private YahooHybridMapProvider()
	{
	}

	static YahooHybridMapProvider()
	{
		UrlFormat = "http://maps{0}.yimg.com/hx/tl?v={1}&t=h&.intl={2}&x={3}&y={4}&z={5}&r=1";
		Instance = new YahooHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 2) + 1, Version, language, pos.X, (1 << zoom >> 1) - 1 - pos.Y, zoom + 1);
	}
}
