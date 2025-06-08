using System;

namespace GMap.NET.MapProviders;

public class YahooMapProvider : YahooMapProviderBase
{
	public static readonly YahooMapProvider Instance;

	public string Version = "2.1";

	private string rnd1 = Guid.NewGuid().ToString("N").Substring(0, 28);

	private string rnd2 = Guid.NewGuid().ToString("N").Substring(0, 20);

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("65DB032C-6869-49B0-A7FC-3AE41A26AF4D");


	public override string Name { get; } = "YahooMap";


	private YahooMapProvider()
	{
	}

	static YahooMapProvider()
	{
		UrlFormat = "http://{0}.base.maps.api.here.com/maptile/{1}/maptile/newest/normal.day/{2}/{3}/{4}/256/png8?lg={5}&token={6}&requestid=yahoo.prod&app_id={7}";
		Instance = new YahooMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 2) + 1, Version, zoom, pos.X, pos.Y, language, rnd1, rnd2);
	}
}
