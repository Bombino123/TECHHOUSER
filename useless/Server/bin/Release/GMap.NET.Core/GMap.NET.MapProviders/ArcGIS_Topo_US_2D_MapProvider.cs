using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_Topo_US_2D_MapProvider : ArcGISMapPlateCarreeProviderBase
{
	public static readonly ArcGIS_Topo_US_2D_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("7652CC72-5C92-40F5-B572-B8FEAA728F6D");


	public override string Name { get; } = "ArcGIS_Topo_US_2D_Map";


	private ArcGIS_Topo_US_2D_MapProvider()
	{
	}

	static ArcGIS_Topo_US_2D_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/NGS_Topo_US_2D/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_Topo_US_2D_MapProvider();
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
