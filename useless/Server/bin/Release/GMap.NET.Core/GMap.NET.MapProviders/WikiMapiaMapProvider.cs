using System;

namespace GMap.NET.MapProviders;

public class WikiMapiaMapProvider : WikiMapiaMapProviderBase
{
	public static readonly WikiMapiaMapProvider Instance;

	private GMapProvider[] _overlays;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("7974022B-1AA6-41F1-8D01-F49940E4B48C");


	public override string Name { get; } = "WikiMapiaMap";


	public override GMapProvider[] Overlays
	{
		get
		{
			if (_overlays == null)
			{
				_overlays = new GMapProvider[1] { this };
			}
			return _overlays;
		}
	}

	private WikiMapiaMapProvider()
	{
	}

	static WikiMapiaMapProvider()
	{
		UrlFormat = "http://i{0}.wikimapia.org/?x={1}&y={2}&zoom={3}";
		Instance = new WikiMapiaMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, string.Empty);
		return GetTileImageUsingHttp(url);
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		return string.Format(UrlFormat, WikiMapiaMapProviderBase.GetServerNum(pos), pos.X, pos.Y, zoom);
	}
}
