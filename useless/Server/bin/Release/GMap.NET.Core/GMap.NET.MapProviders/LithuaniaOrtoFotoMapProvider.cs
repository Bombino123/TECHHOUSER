using System;

namespace GMap.NET.MapProviders;

public class LithuaniaOrtoFotoMapProvider : LithuaniaMapProviderBase
{
	public static readonly LithuaniaOrtoFotoMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("043FF9EF-612C-411F-943C-32C787A88D6A");


	public override string Name { get; } = "LithuaniaOrtoFotoMap";


	private LithuaniaOrtoFotoMapProvider()
	{
	}

	static LithuaniaOrtoFotoMapProvider()
	{
		UrlFormat = "http://dc5.maps.lt/cache/mapslt_ortofoto/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg";
		Instance = new LithuaniaOrtoFotoMapProvider();
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
