using System;

namespace GMap.NET.MapProviders;

public class CzechTuristMapProviderOld : CzechMapProviderBaseOld
{
	public static readonly CzechTuristMapProviderOld Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("B923C81D-880C-42EB-88AB-AF8FE42B564D");


	public override string Name { get; } = "CzechTuristOldMap";


	private CzechTuristMapProviderOld()
	{
	}

	static CzechTuristMapProviderOld()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/turist/{1}_{2:x7}_{3:x7}";
		Instance = new CzechTuristMapProviderOld();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		long num = pos.X << 28 - zoom;
		long num2 = (long)Math.Pow(2.0, zoom) - 1 - pos.Y << 28 - zoom;
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 3) + 1, zoom, num, num2);
	}
}
