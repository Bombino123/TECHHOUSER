using System;

namespace GMap.NET.MapProviders;

public class CzechHybridMapProviderOld : CzechMapProviderBaseOld
{
	public static readonly CzechHybridMapProviderOld Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("F785D98E-DD1D-46FD-8BC1-1AAB69604980");


	public override string Name { get; } = "CzechHybridOldMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					CzechSatelliteMapProviderOld.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private CzechHybridMapProviderOld()
	{
	}

	static CzechHybridMapProviderOld()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/hybrid/{1}_{2:x7}_{3:x7}";
		Instance = new CzechHybridMapProviderOld();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		long num = pos.X << 28 - zoom;
		long num2 = (long)Math.Pow(2.0, zoom) - 1 - pos.Y << 28 - zoom;
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 3) + 1, zoom, num, num2);
	}
}
