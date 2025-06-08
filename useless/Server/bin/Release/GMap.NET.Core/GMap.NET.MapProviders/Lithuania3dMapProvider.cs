using System;

namespace GMap.NET.MapProviders;

public class Lithuania3dMapProvider : LithuaniaMapProviderBase
{
	public static readonly Lithuania3dMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("CCC5B65F-C8BC-47CE-B39D-5E262E6BF083");


	public override string Name { get; } = "Lithuania 2.5d Map";


	private Lithuania3dMapProvider()
	{
	}

	static Lithuania3dMapProvider()
	{
		UrlFormat = "http://dc1.maps.lt/cache/mapslt_25d_vkkp/map/_alllayers/L{0:00}/R{1:x8}/C{2:x8}.png";
		Instance = new Lithuania3dMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		int num = zoom;
		if (zoom >= 10)
		{
			num -= 10;
		}
		return string.Format(UrlFormat, num, pos.Y, pos.X);
	}
}
