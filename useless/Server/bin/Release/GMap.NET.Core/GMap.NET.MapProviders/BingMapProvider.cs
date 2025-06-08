using System;

namespace GMap.NET.MapProviders;

public class BingMapProvider : BingMapProviderBase
{
	public static readonly BingMapProvider Instance;

	private string _urlDynamicFormat = string.Empty;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("D0CEB371-F10A-4E12-A2C1-DF617D6674A8");


	public override string Name { get; } = "BingMap";


	private BingMapProvider()
	{
	}

	static BingMapProvider()
	{
		UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}?g={2}&mkt={3}&lbl=l1&stl=h&shading=hill&n=z{4}";
		Instance = new BingMapProvider();
	}

	public override PureImage GetTileImage(GPoint pos, int zoom)
	{
		string url = MakeTileImageUrl(pos, zoom, GMapProvider.LanguageStr);
		return GetTileImageUsingHttp(url);
	}

	public override void OnInitialized()
	{
		base.OnInitialized();
		if (!DisableDynamicTileUrlFormat)
		{
			_urlDynamicFormat = GetTileUrl("Road");
			if (!string.IsNullOrEmpty(_urlDynamicFormat))
			{
				_urlDynamicFormat = _urlDynamicFormat.Replace("{subdomain}", "t{0}").Replace("{quadkey}", "{1}").Replace("{culture}", "{2}");
			}
		}
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		string text = TileXYToQuadKey(pos.X, pos.Y, zoom);
		if (!DisableDynamicTileUrlFormat && !string.IsNullOrEmpty(_urlDynamicFormat))
		{
			return string.Format(_urlDynamicFormat, GMapProvider.GetServerNum(pos, 4), text, language);
		}
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 4), text, Version, language, ForceSessionIdOnTileAccess ? ("&key=" + SessionId) : string.Empty);
	}
}
