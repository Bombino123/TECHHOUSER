using System;

namespace GMap.NET.MapProviders;

public class CzechMapProviderOld : CzechMapProviderBaseOld
{
	public static readonly CzechMapProviderOld Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("6A1AF99A-84C6-4EF6-91A5-77B9D03257C2");


	public override string Name { get; } = "CzechOldMap";


	private CzechMapProviderOld()
	{
	}

	static CzechMapProviderOld()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/base-n/{1}_{2:x7}_{3:x7}";
		Instance = new CzechMapProviderOld();
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
