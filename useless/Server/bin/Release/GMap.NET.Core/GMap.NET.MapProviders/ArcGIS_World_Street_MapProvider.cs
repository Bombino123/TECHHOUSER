using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_World_Street_MapProvider : ArcGISMapMercatorProviderBase
{
	public static readonly ArcGIS_World_Street_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("E1FACDF6-E535-4D69-A49F-12B623A467A9");


	public override string Name { get; } = "ArcGIS_World_Street_Map";


	private ArcGIS_World_Street_MapProvider()
	{
	}

	static ArcGIS_World_Street_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_World_Street_MapProvider();
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
