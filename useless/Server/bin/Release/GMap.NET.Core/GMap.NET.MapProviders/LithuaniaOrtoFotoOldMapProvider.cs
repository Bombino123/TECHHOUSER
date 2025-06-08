using System;

namespace GMap.NET.MapProviders;

public class LithuaniaOrtoFotoOldMapProvider : LithuaniaMapProviderBase
{
	public static readonly LithuaniaOrtoFotoOldMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("C37A148E-0A7D-4123-BE4E-D0D3603BE46B");


	public override string Name { get; } = "LithuaniaOrtoFotoMapOld";


	private LithuaniaOrtoFotoOldMapProvider()
	{
	}

	static LithuaniaOrtoFotoOldMapProvider()
	{
		UrlFormat = "http://dc1.maps.lt/cache/mapslt_ortofoto_2010/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.jpg";
		Instance = new LithuaniaOrtoFotoOldMapProvider();
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
