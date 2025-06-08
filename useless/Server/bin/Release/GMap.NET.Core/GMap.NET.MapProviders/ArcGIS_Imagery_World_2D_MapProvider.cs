using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_Imagery_World_2D_MapProvider : ArcGISMapPlateCarreeProviderBase
{
	public static readonly ArcGIS_Imagery_World_2D_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("FF7ADDAD-F155-41DB-BC42-CC6FD97C8B9D");


	public override string Name { get; } = "ArcGIS_Imagery_World_2D_Map";


	private ArcGIS_Imagery_World_2D_MapProvider()
	{
	}

	static ArcGIS_Imagery_World_2D_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_Imagery_World_2D/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_Imagery_World_2D_MapProvider();
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
