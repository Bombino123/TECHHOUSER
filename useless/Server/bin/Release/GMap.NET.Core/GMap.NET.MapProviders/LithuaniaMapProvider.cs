using System;

namespace GMap.NET.MapProviders;

public class LithuaniaMapProvider : LithuaniaMapProviderBase
{
	public static readonly LithuaniaMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("5859079F-1B5E-484B-B05C-41CE664D8A93");


	public override string Name { get; } = "LithuaniaMap";


	private LithuaniaMapProvider()
	{
	}

	static LithuaniaMapProvider()
	{
		UrlFormat = "http://dc5.maps.lt/cache/mapslt/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png";
		Instance = new LithuaniaMapProvider();
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
