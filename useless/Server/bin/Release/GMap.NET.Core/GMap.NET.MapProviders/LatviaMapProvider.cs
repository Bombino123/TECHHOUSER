using System;

namespace GMap.NET.MapProviders;

public class LatviaMapProvider : LatviaMapProviderBase
{
	public static readonly LatviaMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("2A21CBB1-D37C-458D-905E-05F19536EF1F");


	public override string Name { get; } = "LatviaMap";


	private LatviaMapProvider()
	{
	}

	static LatviaMapProvider()
	{
		UrlFormat = "http://services.maps.lt/mapsk_services/rest/services/ikartelv/MapServer/tile/{0}/{1}/{2}.png?cl=ikrlv";
		Instance = new LatviaMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, zoom, pos.Y, pos.X);
	}
}
