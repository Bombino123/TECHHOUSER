using System;

namespace GMap.NET.MapProviders;

public class CzechHybridMapProvider : CzechMapProviderBase
{
	public static readonly CzechHybridMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("7540CE5B-F634-41E9-B23E-A6E0A97526FD");


	public override string Name { get; } = "CzechHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					CzechSatelliteMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private CzechHybridMapProvider()
	{
	}

	static CzechHybridMapProvider()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/hybrid-m/{1}-{2}-{3}";
		Instance = new CzechHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 3) + 1, zoom, pos.X, pos.Y);
	}
}
