using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders;

public class UMPMapProvider : GMapProvider
{
	public static readonly UMPMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("E36E311E-256A-4639-9AF7-FEB7BDEA6ABE");


	public override string Name { get; } = "UMP";


	public override PureProjection Projection => MercatorProjection.Instance;

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

	private UMPMapProvider()
	{
		RefererUrl = "http://ump.waw.pl/";
		Copyright = "Data by UMP-pcPL";
	}

	static UMPMapProvider()
	{
		UrlFormat = "http://tiles.ump.waw.pl/ump_tiles/{0}/{1}/{2}.png";
		Instance = new UMPMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, zoom, pos.X, pos.Y);
	}
}
