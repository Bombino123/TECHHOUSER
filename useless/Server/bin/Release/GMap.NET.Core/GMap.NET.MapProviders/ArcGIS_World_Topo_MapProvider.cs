using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_World_Topo_MapProvider : ArcGISMapMercatorProviderBase
{
	public static readonly ArcGIS_World_Topo_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("E0354A49-7447-4C9A-814F-A68565ED834B");


	public override string Name { get; } = "ArcGIS_World_Topo_Map";


	private ArcGIS_World_Topo_MapProvider()
	{
	}

	static ArcGIS_World_Topo_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_World_Topo_MapProvider();
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
