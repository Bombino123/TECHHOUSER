using System;

namespace GMap.NET.MapProviders;

public class GoogleTerrainMapProvider : GoogleMapProviderBase
{
	public static readonly GoogleTerrainMapProvider Instance;

	public string Version = "t@132,r@333000000";

	private static readonly string UrlFormatServer;

	private static readonly string UrlFormatRequest;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("A42EDF2E-63C5-4967-9DBF-4EFB3AF7BC11");


	public override string Name { get; } = "GoogleTerrainMap";


	private GoogleTerrainMapProvider()
	{
	}

	static GoogleTerrainMapProvider()
	{
		UrlFormatServer = "mt";
		UrlFormatRequest = "vt";
		UrlFormat = "https://{0}{1}.{10}/maps/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
		Instance = new GoogleTerrainMapProvider();
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
