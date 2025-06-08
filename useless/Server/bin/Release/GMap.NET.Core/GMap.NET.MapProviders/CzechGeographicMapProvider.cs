using System;

namespace GMap.NET.MapProviders;

public class CzechGeographicMapProvider : CzechMapProviderBase
{
	public static readonly CzechGeographicMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("50EC9FCC-E4D7-4F53-8700-2D1DB73A1D48");


	public override string Name { get; } = "CzechGeographicMap";


	private CzechGeographicMapProvider()
	{
	}

	static CzechGeographicMapProvider()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/zemepis-m/{1}-{2}-{3}";
		Instance = new CzechGeographicMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 3) + 1, zoom, pos.X, pos.Y);
	}
}
