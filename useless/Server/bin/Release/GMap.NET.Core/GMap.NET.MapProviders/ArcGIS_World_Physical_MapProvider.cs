using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_World_Physical_MapProvider : ArcGISMapMercatorProviderBase
{
	public static readonly ArcGIS_World_Physical_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("0C0E73E3-5EA6-4F08-901C-AE85BCB1BFC8");


	public override string Name { get; } = "ArcGIS_World_Physical_Map";


	private ArcGIS_World_Physical_MapProvider()
	{
	}

	static ArcGIS_World_Physical_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_World_Physical_MapProvider();
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
