using System;

namespace GMap.NET.MapProviders;

public class OpenStreet4UMapProvider : OpenStreetMapProviderBase
{
	public static readonly OpenStreet4UMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("3E3D919E-9814-4978-B430-6AAB2C1E41B2");


	public override string Name { get; } = "OpenStreet4UMap";


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

	private OpenStreet4UMapProvider()
	{
		RefererUrl = "http://www.4umaps.eu/map.htm";
		Copyright = $"© 4UMaps.eu, © OpenStreetMap - Map data ©{DateTime.Today.Year} OpenStreetMap";
	}

	static OpenStreet4UMapProvider()
	{
		UrlFormat = "http://4umaps.eu/{0}/{1}/{2}.png";
		Instance = new OpenStreet4UMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom)
	{
		return string.Format(UrlFormat, zoom, pos.X, pos.Y);
	}
}
