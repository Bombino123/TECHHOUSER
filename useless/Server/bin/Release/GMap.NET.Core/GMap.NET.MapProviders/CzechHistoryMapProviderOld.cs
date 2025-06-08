using System;

namespace GMap.NET.MapProviders;

public class CzechHistoryMapProviderOld : CzechMapProviderBaseOld
{
	public static readonly CzechHistoryMapProviderOld Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("C666AAF4-9D27-418F-97CB-7F0D8CC44544");


	public override string Name { get; } = "CzechHistoryOldMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					this,
					CzechHybridMapProviderOld.Instance
				};
			}
			return _overlays;
		}
	}

	private CzechHistoryMapProviderOld()
	{
	}

	static CzechHistoryMapProviderOld()
	{
		UrlFormat = "http://m{0}.mapserver.mapy.cz/army2/{1}_{2:x7}_{3:x7}";
		Instance = new CzechHistoryMapProviderOld();
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
