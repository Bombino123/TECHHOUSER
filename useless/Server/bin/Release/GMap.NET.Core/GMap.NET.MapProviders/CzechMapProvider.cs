using System;

namespace GMap.NET.MapProviders;

public class CzechMapProvider : CzechMapProviderBase
{
	public static readonly CzechMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("13AB92EF-8C3B-4FAC-B2CD-2594C05F8BFC");


	public override string Name { get; } = "CzechMap";


	private CzechMapProvider()
	{
	}

	static CzechMapProvider()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/base-m/{1}-{2}-{3}";
		Instance = new CzechMapProvider();
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
