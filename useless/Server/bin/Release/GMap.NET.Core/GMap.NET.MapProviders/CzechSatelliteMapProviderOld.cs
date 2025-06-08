using System;

namespace GMap.NET.MapProviders;

public class CzechSatelliteMapProviderOld : CzechMapProviderBaseOld
{
	public static readonly CzechSatelliteMapProviderOld Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("7846D655-5F9C-4042-8652-60B6BF629C3C");


	public override string Name { get; } = "CzechSatelliteOldMap";


	private CzechSatelliteMapProviderOld()
	{
	}

	static CzechSatelliteMapProviderOld()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/ophoto/{1}_{2:x7}_{3:x7}";
		Instance = new CzechSatelliteMapProviderOld();
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
