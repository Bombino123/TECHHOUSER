using System;

namespace GMap.NET.MapProviders;

public class ArcGIS_World_Shaded_Relief_MapProvider : ArcGISMapMercatorProviderBase
{
	public static readonly ArcGIS_World_Shaded_Relief_MapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("2E821FEF-8EA1-458A-BC82-4F699F4DEE79");


	public override string Name { get; } = "ArcGIS_World_Shaded_Relief_Map";


	private ArcGIS_World_Shaded_Relief_MapProvider()
	{
	}

	static ArcGIS_World_Shaded_Relief_MapProvider()
	{
		UrlFormat = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Shaded_Relief/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_World_Shaded_Relief_MapProvider();
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
