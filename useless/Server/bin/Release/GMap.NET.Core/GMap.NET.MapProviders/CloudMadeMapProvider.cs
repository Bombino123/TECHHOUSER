using System;

namespace GMap.NET.MapProviders;

public class CloudMadeMapProvider : CloudMadeMapProviderBase
{
	public static readonly CloudMadeMapProvider Instance;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("00403A36-725F-4BC4-934F-BFC1C164D003");


	public override string Name { get; } = "CloudMade, Demo";


	private CloudMadeMapProvider()
	{
		Key = "5937c2bd907f4f4a92d8980a7c666ac0";
		StyleID = 45363;
	}

	static CloudMadeMapProvider()
	{
		UrlFormat = "http://{0}.tile.cloudmade.com/{1}/{2}{3}/256/{4}/{5}/{6}.png";
		Instance = new CloudMadeMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, ServerLetters[GMapProvider.GetServerNum(pos, 3)], Key, StyleID, DoubleResolution ? DoubleResolutionString : string.Empty, zoom, pos.X, pos.Y);
	}
}
