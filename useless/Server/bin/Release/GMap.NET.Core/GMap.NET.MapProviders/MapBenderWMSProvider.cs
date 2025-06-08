using System;
using System.Globalization;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class MapBenderWMSProvider : GMapProvider
{
	public static readonly MapBenderWMSProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("45742F8D-B552-4CAF-89AE-F20951BBDB2B");


	public override string Name { get; } = "MapBender, WMS demo";


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

	public override PureProjection Projection => MercatorProjection.Instance;

	private MapBenderWMSProvider()
	{
	}

	static MapBenderWMSProvider()
	{
		UrlFormat = "http://mapbender.wheregroup.com/cgi-bin/mapserv?map=/data/umn/osm/osm_basic.map&VERSION=1.1.1&REQUEST=GetMap&SERVICE=WMS&LAYERS=OSM_Basic&styles=&bbox={0},{1},{2},{3}&width={4}&height={5}&srs=EPSG:4326&format=image/png";
		Instance = new MapBenderWMSProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		GPoint gPoint = Projection.FromTileXYToPixel(pos);
		GPoint p = gPoint;
		gPoint.Offset(0L, Projection.TileSize.Height);
		PointLatLng pointLatLng = Projection.FromPixelToLatLng(gPoint, zoom);
		p.Offset(Projection.TileSize.Width, 0L);
		PointLatLng pointLatLng2 = Projection.FromPixelToLatLng(p, zoom);
		return string.Format(CultureInfo.InvariantCulture, UrlFormat, pointLatLng.Lng, pointLatLng.Lat, pointLatLng2.Lng, pointLatLng2.Lat, Projection.TileSize.Width, Projection.TileSize.Height);
	}
}
