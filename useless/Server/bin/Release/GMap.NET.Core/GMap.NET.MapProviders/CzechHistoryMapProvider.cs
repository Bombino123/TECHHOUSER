using System;

namespace GMap.NET.MapProviders;

public class CzechHistoryMapProvider : CzechMapProviderBase
{
	public static readonly CzechHistoryMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("CD44C19D-5EED-4623-B367-FB39FDC55B8F");


	public override string Name { get; } = "CzechHistoryMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					this,
					CzechHybridMapProvider.Instance
				};
			}
			return _overlays;
		}
	}

	private CzechHistoryMapProvider()
	{
		MaxZoom = 15;
	}

	static CzechHistoryMapProvider()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/army2-m/{1}-{2}-{3}";
		Instance = new CzechHistoryMapProvider();
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
