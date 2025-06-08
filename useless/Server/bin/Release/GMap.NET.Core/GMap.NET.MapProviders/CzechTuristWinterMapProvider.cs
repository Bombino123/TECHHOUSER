using System;

namespace GMap.NET.MapProviders;

public class CzechTuristWinterMapProvider : CzechMapProviderBase
{
	public static readonly CzechTuristWinterMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("F7B7FC9E-BDC2-4A9D-A1D3-A6BEC8FE0EB2");


	public override string Name { get; } = "CzechTuristWinterMap";


	private CzechTuristWinterMapProvider()
	{
	}

	static CzechTuristWinterMapProvider()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/wturist_winter-m/{1}-{2}-{3}";
		Instance = new CzechTuristWinterMapProvider();
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
