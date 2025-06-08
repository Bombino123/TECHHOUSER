using System;

namespace GMap.NET.MapProviders;

public class GoogleKoreaSatelliteMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleKoreaSatelliteMapProvider Instance;

	public string Version = "170";

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("70370941-D70C-4123-BE4A-AEE6754047F5");


	public override string Name { get; } = "GoogleKoreaSatelliteMap";


	private GoogleKoreaSatelliteMapProvider()
	{
	}

	static GoogleKoreaSatelliteMapProvider()
	{
		UrlFormatServer = "khm";
		UrlFormatRequest = "kh";
		UrlFormat = "https://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleKoreaSatelliteMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		GetSecureWords(pos, out var sec, out var sec2);
		return string.Format(UrlFormat, UrlFormatServer, GMapProvider.GetServerNum(pos, 4), UrlFormatRequest, Version, language, pos.X, sec, pos.Y, zoom, sec2, ServerKoreaKr);
	}
}
