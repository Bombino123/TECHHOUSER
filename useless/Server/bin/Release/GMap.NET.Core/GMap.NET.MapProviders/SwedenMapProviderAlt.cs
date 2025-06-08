using System;

namespace GMap.NET.MapProviders;

public class SwedenMapProviderAlt : SwedenMapProviderAltBase
{
	public static readonly SwedenMapProviderAlt Instance;

	private readonly Guid id = new Guid("d5e8e0de-3a93-4983-941e-9b66d79f50d6");

	private readonly string name = "SwedenMapAlternative";

	private static readonly string UrlFormat;

	public override Guid Id => id;

	public override string Name => name;

	private SwedenMapProviderAlt()
	{
	}

	static SwedenMapProviderAlt()
	{
		UrlFormat = "https://kso.etjanster.lantmateriet.se/karta/topowebb/v1.1/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=topowebb&STYLE=default&TILEMATRIXSET=3857&TILEMATRIX={0}&TILEROW={1}&TILECOL={2}&FORMAT=image%2Fpng";
		Instance = new SwedenMapProviderAlt();
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
