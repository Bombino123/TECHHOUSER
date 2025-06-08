using System;

namespace GMap.NET.MapProviders;

public class SwedenMapProvider : SwedenMapProviderBase
{
	public static readonly SwedenMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("40890A96-6E82-4FA7-90A3-73D66B974F63");


	public override string Name { get; } = "SwedenMap";


	private SwedenMapProvider()
	{
	}

	static SwedenMapProvider()
	{
		UrlFormat = "https://kso.etjanster.lantmateriet.se/karta/topowebb/v1.1/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=topowebb&STYLE=default&TILEMATRIXSET=3006&TILEMATRIX={0}&TILEROW={1}&TILECOL={2}&FORMAT=image%2Fpng";
		Instance = new SwedenMapProvider();
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
