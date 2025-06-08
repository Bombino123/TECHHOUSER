using System;

namespace GMap.NET.MapProviders;

public class CzechSatelliteMapProvider : CzechMapProviderBase
{
	public static readonly CzechSatelliteMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("30F433DB-BBF5-463D-9AB5-76383483B605");


	public override string Name { get; } = "CzechSatelliteMap";


	private CzechSatelliteMapProvider()
	{
	}

	static CzechSatelliteMapProvider()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/ophoto-m/{1}-{2}-{3}";
		Instance = new CzechSatelliteMapProvider();
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
