using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class SpainMapProvider : GMapProvider
{
	public static readonly SpainMapProvider Instance;

	private readonly string[] _levels = new string[20]
	{
		"0", "1", "2", "3", "4", "MTNSIGPAC", "MTN2000", "MTN2000", "MTN2000", "MTN2000",
		"MTN2000", "MTN200", "MTN200", "MTN200", "MTN25", "MTN25", "ORTOFOTOS", "ORTOFOTOS", "ORTOFOTOS", "ORTOFOTOS"
	};

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("7B70ABB0-1265-4D34-9442-F0788F4F689F");


	public override string Name { get; } = "SpainMap";


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

	private SpainMapProvider()
	{
		Copyright = $"©{DateTime.Today.Year} SIGPAC";
		MinZoom = 5;
		Area = new RectLatLng(43.8741381814747, -9.700927734375, 14.34814453125, 7.8605775962932);
	}

	static SpainMapProvider()
	{
		UrlFormat = "http://sigpac.mapa.es/kmlserver/raster/{0}@3785/{1}.{2}.{3}.img";
		Instance = new SpainMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, _levels[zoom], zoom, pos.X, (2 << zoom - 1) - pos.Y - 1);
	}
}
