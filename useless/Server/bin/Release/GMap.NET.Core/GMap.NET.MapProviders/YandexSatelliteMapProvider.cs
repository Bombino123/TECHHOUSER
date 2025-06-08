using System;

namespace GMap.NET.MapProviders;

public class YandexSatelliteMapProvider : YandexMapProviderBase
{
	public static readonly YandexSatelliteMapProvider Instance;

	public new string Version = "3.135.0";

	private static readonly string UrlServer;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("2D4CE763-0F91-40B2-A511-13EF428237AD");


	public override string Name { get; } = "YandexSatelliteMap";


	private YandexSatelliteMapProvider()
	{
	}

	static YandexSatelliteMapProvider()
	{
		UrlServer = "sat";
		UrlFormat = "http://{0}0{1}.{7}/tiles?l=sat&v={2}&x={3}&y={4}&z={5}&lang={6}";
		Instance = new YandexSatelliteMapProvider();
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
