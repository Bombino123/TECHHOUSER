using System;

namespace GMap.NET.MapProviders;

public class GoogleKoreaHybridMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleKoreaHybridMapProvider Instance;

	public string Version = "kr1t.12";

	private GMapProvider[] _overlays;

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("41A91842-04BC-442B-9AC8-042156238A5B");


	public override string Name { get; } = "GoogleKoreaHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					GoogleKoreaSatelliteMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private GoogleKoreaHybridMapProvider()
	{
	}

	static GoogleKoreaHybridMapProvider()
	{
		UrlFormatServer = "mt";
		UrlFormatRequest = "mt";
		UrlFormat = "https://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleKoreaHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		GetSecureWords(pos, out var sec, out var sec2);
		return string.Format(UrlFormat, UrlFormatServer, GMapProvider.GetServerNum(pos, 4), UrlFormatRequest, Version, language, pos.X, sec, pos.Y, zoom, sec2, ServerKorea);
	}
}
