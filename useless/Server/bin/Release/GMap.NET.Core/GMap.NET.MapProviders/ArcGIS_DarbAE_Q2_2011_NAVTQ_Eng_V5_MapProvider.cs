using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider : GMapProvider
{
	public static readonly ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("E03CFEDF-9277-49B3-9912-D805347F934B");


	public override string Name { get; } = "ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider";


	public override PureProjection Projection => PlateCarreeProjectionDarbAe.Instance;

	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[1] { this };
			}
			return _overlays;
		}
	}

	private ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider()
	{
		MaxZoom = 12;
		Area = RectLatLng.FromLTRB(49.8846923723311, 28.0188609585523, 58.2247031977662, 21.154115956732);
		Copyright = string.Format("©{0} ESRI - Map data ©{0} ArcGIS", DateTime.Today.Year);
	}

	static ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider()
	{
		UrlFormat = "http://www.darb.ae/ArcGIS/rest/services/BaseMaps/Q2_2011_NAVTQ_Eng_V5/MapServer/tile/{0}/{1}/{2}";
		Instance = new ArcGIS_DarbAE_Q2_2011_NAVTQ_Eng_V5_MapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom)
	{
		return string.Format(UrlFormat, zoom, pos.Y, pos.X);
	}
}
