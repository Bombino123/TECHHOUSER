using System;

namespace GMap.NET.MapProviders;

public class GoogleChinaHybridMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleChinaHybridMapProvider Instance;

	public string Version = "h@298";

	private GMapProvider[] _overlays;

	private static readonly string ChinaLanguage;

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("B8A2A78D-1C49-45D0-8F03-9B95C83116B7");


	public override string Name { get; } = "GoogleChinaHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					GoogleChinaSatelliteMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private GoogleChinaHybridMapProvider()
	{
		RefererUrl = $"http://ditu.{ServerChina}/";
	}

	static GoogleChinaHybridMapProvider()
	{
		ChinaLanguage = "zh-CN";
		UrlFormatServer = "mt";
		UrlFormatRequest = "vt";
		UrlFormat = "http://{0}{1}.{10}/{2}/imgtp=png32&lyrs={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleChinaHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		GetSecureWords(pos, out var sec, out var sec2);
		return string.Format(UrlFormat, UrlFormatServer, GMapProvider.GetServerNum(pos, 4), UrlFormatRequest, Version, ChinaLanguage, pos.X, sec, pos.Y, zoom, sec2, ServerChina);
	}
}
