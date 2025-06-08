using System;

namespace GMap.NET.MapProviders;

public class GoogleHybridMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleHybridMapProvider Instance;

	public string Version = "h@333000000";

	private GMapProvider[] _overlays;

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("B076C255-6D12-4466-AAE0-4A73D20A7E6A");


	public override string Name { get; } = "GoogleHybridMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[2]
				{
					GoogleSatelliteMapProvider.Instance,
					this
				};
			}
			return _overlays;
		}
	}

	private GoogleHybridMapProvider()
	{
	}

	static GoogleHybridMapProvider()
	{
		UrlFormatServer = "mt";
		UrlFormatRequest = "vt";
		UrlFormat = "http://{0}{1}.{10}/maps/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleHybridMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		GetSecureWords(pos, out var sec, out var sec2);
		return string.Format(UrlFormat, UrlFormatServer, GMapProvider.GetServerNum(pos, 4), UrlFormatRequest, Version, language, pos.X, sec, pos.Y, zoom, sec2, Server);
	}
}
