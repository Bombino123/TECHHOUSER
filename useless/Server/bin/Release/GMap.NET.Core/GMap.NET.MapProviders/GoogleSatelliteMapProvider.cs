using System;

namespace GMap.NET.MapProviders;

public class GoogleSatelliteMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleSatelliteMapProvider Instance;

	public string Version = "192";

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("9CB89D76-67E9-47CF-8137-B9EE9FC46388");


	public override string Name { get; } = "GoogleSatelliteMap";


	private GoogleSatelliteMapProvider()
	{
	}

	static GoogleSatelliteMapProvider()
	{
		UrlFormatServer = "khm";
		UrlFormatRequest = "kh";
		UrlFormat = "https://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleSatelliteMapProvider();
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
