using System;

namespace GMap.NET.MapProviders;

public class YahooSatelliteMapProvider : YahooMapProviderBase
{
	public static readonly YahooSatelliteMapProvider Instance;

	public string Version = "1.9";

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("55D71878-913F-4320-B5B6-B4167A3F148F");


	public override string Name { get; } = "YahooSatelliteMap";


	private YahooSatelliteMapProvider()
	{
	}

	static YahooSatelliteMapProvider()
	{
		UrlFormat = "http://maps{0}.yimg.com/ae/ximg?v={1}&t=a&s=256&.intl={2}&x={3}&y={4}&z={5}&r=1";
		Instance = new YahooSatelliteMapProvider();
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
