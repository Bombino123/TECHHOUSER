using System;

namespace GMap.NET.MapProviders;

public class NearSatelliteMapProvider : NearMapProviderBase
{
	public static readonly NearSatelliteMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("56D00148-05B7-408D-8F7A-8D7250FF8121");


	public override string Name { get; } = "NearSatelliteMap";


	private NearSatelliteMapProvider()
	{
	}

	static NearSatelliteMapProvider()
	{
		UrlFormat = "http://web{0}.nearmap.com/maps/hl=en&x={1}&y={2}&z={3}&nml=Vert{4}";
		Instance = new NearSatelliteMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, NearMapProviderBase.GetServerNum(pos, 4), pos.X, pos.Y, zoom, GetSafeString(pos));
	}
}
