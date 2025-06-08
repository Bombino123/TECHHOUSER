using System;

namespace GMap.NET.MapProviders;

public class NearMapProvider : NearMapProviderBase
{
	public static readonly NearMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("E33803DF-22CB-4FFA-B8E3-15383ED9969D");


	public override string Name { get; } = "NearMap";


	private NearMapProvider()
	{
		RefererUrl = "http://www.nearmap.com/";
	}

	static NearMapProvider()
	{
		UrlFormat = "http://web{0}.nearmap.com/kh/v=nm&hl=en&x={1}&y={2}&z={3}&nml=Map_{4}";
		Instance = new NearMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, NearMapProviderBase.GetServerNum(pos, 3), pos.X, pos.Y, zoom, GetSafeString(pos));
	}
}
