using System;

namespace GMap.NET.MapProviders;

public class GoogleKoreaMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleKoreaMapProvider Instance;

	public string Version = "kr1.12";

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("0079D360-CB1B-4986-93D5-AD299C8E20E6");


	public override string Name { get; } = "GoogleKoreaMap";


	private GoogleKoreaMapProvider()
	{
		Area = new RectLatLng(38.6597777307125, 125.738525390625, 4.02099609375, 4.42072406219614);
	}

	static GoogleKoreaMapProvider()
	{
		UrlFormatServer = "mt";
		UrlFormatRequest = "mt";
		UrlFormat = "https://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleKoreaMapProvider();
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
