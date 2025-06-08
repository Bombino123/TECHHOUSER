using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_ShadedRelief_World_2D_MapProvider : ArcGISMapPlateCarreeProviderBase
{
	public static readonly ArcGIS_ShadedRelief_World_2D_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("A8995FA4-D9D8-415B-87D0-51A7E53A90D4");


	public override string Name { get; } = "ArcGIS_ShadedRelief_World_2D_Map";


	private ArcGIS_ShadedRelief_World_2D_MapProvider()
	{
	}

	static ArcGIS_ShadedRelief_World_2D_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/ESRI_ShadedRelief_World_2D/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_ShadedRelief_World_2D_MapProvider();
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
