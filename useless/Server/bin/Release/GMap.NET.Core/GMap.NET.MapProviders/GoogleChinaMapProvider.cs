using System;

namespace GMap.NET.MapProviders;

public class GoogleChinaMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleChinaMapProvider Instance;

	public string Version = "m@298";

	private static readonly string ChinaLanguage;

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("1213F763-64EE-4AB6-A14A-D84D6BCC3426");


	public override string Name { get; } = "GoogleChinaMap";


	private GoogleChinaMapProvider()
	{
		RefererUrl = $"http://ditu.{ServerChina}/";
	}

	static GoogleChinaMapProvider()
	{
		ChinaLanguage = "zh-CN";
		UrlFormatServer = "mt";
		UrlFormatRequest = "vt";
		UrlFormat = "http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleChinaMapProvider();
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
