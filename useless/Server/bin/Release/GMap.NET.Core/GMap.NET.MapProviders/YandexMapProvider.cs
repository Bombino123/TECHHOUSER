using System;

namespace GMap.NET.MapProviders;

public class YandexMapProvider : YandexMapProviderBase
{
	public static readonly YandexMapProvider Instance;

	private static readonly string UrlServer;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("82DC969D-0491-40F3-8C21-4D90B67F47EB");


	public override string Name { get; } = "YandexMap";


	private YandexMapProvider()
	{
		RefererUrl = "http://" + ServerCom + "/";
	}

	static YandexMapProvider()
	{
		UrlServer = "vec";
		UrlFormat = "http://{0}0{1}.{7}/tiles?l=map&v={2}&x={3}&y={4}&z={5}&lang={6}";
		Instance = new YandexMapProvider();
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
