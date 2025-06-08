using System;

namespace GMap.NET.MapProviders;

public class CzechTuristMapProvider : CzechMapProviderBase
{
	public static readonly CzechTuristMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("102A54BE-3894-439B-9C1F-CA6FF2EA1FE9");


	public override string Name { get; } = "CzechTuristMap";


	private CzechTuristMapProvider()
	{
	}

	static CzechTuristMapProvider()
	{
		UrlFormat = "https://mapserver.mapy.cz/turist-m/{1}-{2}-{3}";
		Instance = new CzechTuristMapProvider();
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
