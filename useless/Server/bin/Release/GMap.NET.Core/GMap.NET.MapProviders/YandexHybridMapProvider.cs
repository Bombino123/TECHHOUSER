using System;

namespace GMap.NET.MapProviders;

public class YandexHybridMapProvider : YandexMapProviderBase
{
	public static readonly YandexHybridMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlServer;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("78A3830F-5EE3-432C-A32E-91B7AF6BBCB9");


	public override string Name { get; } = "YandexHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					YandexSatelliteMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private YandexHybridMapProvider()
	{
	}

	static YandexHybridMapProvider()
	{
		UrlServer = "vec";
		UrlFormat = "http://{0}0{1}.{7}/tiles?l=skl&v={2}&x={3}&y={4}&z={5}&lang={6}";
		Instance = new YandexHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, UrlServer, GMapProvider.GetServerNum(pos, 4) + 1, Version, pos.X, pos.Y, zoom, language, Server);
	}
}
