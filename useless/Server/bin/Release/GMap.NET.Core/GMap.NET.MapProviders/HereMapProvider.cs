using System;

namespace GMap.NET.MapProviders;

public class HereMapProvider : HereMapProviderBase
{
	public static readonly HereMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("30DC2083-AC4D-4471-A232-D8A67AC9373A");


	public override string Name { get; } = "HereMap";


	private HereMapProvider()
	{
	}

	static HereMapProvider()
	{
		UrlFormat = "http://{0}.traffic.maps.cit.api.here.com/maptile/2.1/traffictile/newest/normal.day/{1}/{2}/{3}/256/png8?app_id={4}&app_code={5}";
		Instance = new HereMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, HereMapProviderBase.UrlServerLetters[GMapProvider.GetServerNum(pos, 4)], zoom, pos.X, pos.Y, AppId, AppCode);
	}
}
