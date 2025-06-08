using System;

namespace GMap.NET.MapProviders;

public class GoogleChinaSatelliteMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleChinaSatelliteMapProvider Instance;

	public string Version = "s@170";

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("543009AC-3379-4893-B580-DBE6372B1753");


	public override string Name { get; } = "GoogleChinaSatelliteMap";


	private GoogleChinaSatelliteMapProvider()
	{
		RefererUrl = $"http://ditu.{ServerChina}/";
	}

	static GoogleChinaSatelliteMapProvider()
	{
		UrlFormatServer = "mt";
		UrlFormatRequest = "vt";
		UrlFormat = "http://{0}{1}.{9}/{2}/lyrs={3}&gl=cn&x={4}{5}&y={6}&z={7}&s={8}";
		Instance = new GoogleChinaSatelliteMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		GetSecureWords(pos, out var sec, out var sec2);
		return string.Format(UrlFormat, UrlFormatServer, GMapProvider.GetServerNum(pos, 4), UrlFormatRequest, Version, pos.X, sec, pos.Y, zoom, sec2, ServerChina);
	}
}
