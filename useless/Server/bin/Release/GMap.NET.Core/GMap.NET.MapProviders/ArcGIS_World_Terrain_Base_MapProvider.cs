using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_World_Terrain_Base_MapProvider : ArcGISMapMercatorProviderBase
{
	public static readonly ArcGIS_World_Terrain_Base_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("927F175B-5200-4D95-A99B-1C87C93099DA");


	public override string Name { get; } = "ArcGIS_World_Terrain_Base_Map";


	private ArcGIS_World_Terrain_Base_MapProvider()
	{
	}

	static ArcGIS_World_Terrain_Base_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Terrain_Base/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_World_Terrain_Base_MapProvider();
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
