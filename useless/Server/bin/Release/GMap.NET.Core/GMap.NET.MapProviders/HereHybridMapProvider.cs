using System;

namespace GMap.NET.MapProviders;

public class HereHybridMapProvider : HereMapProviderBase
{
	public static readonly HereHybridMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("B85A8FD7-40F4-40EE-9B45-491AA45D86C1");


	public override string Name { get; } = "HereHybridMap";


	private HereHybridMapProvider()
	{
	}

	static HereHybridMapProvider()
	{
		UrlFormat = "http://{0}.traffic.maps.cit.api.here.com/maptile/2.1/traffictile/newest/hybrid.day/{1}/{2}/{3}/256/png8?app_id={4}&app_code={5}";
		Instance = new HereHybridMapProvider();
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
