using System;

namespace GMap.NET.MapProviders;

public class BingOSMapProvider : BingMapProviderBase
{
	public static readonly BingOSMapProvider Instance;

	private string _urlDynamicFormat = string.Empty;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("3C12C212-A79F-42D0-9A1B-22740E1103E8");


	public override string Name { get; } = "BingOSMap";


	private BingOSMapProvider()
	{
	}

	static BingOSMapProvider()
	{
		UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.jpeg?g={2}&mkt={3}&n=z{4}&productSet=mmOS";
		Instance = new BingOSMapProvider();
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
			_urlDynamicFormat = GetTileUrl("OrdnanceSurvey");
			if (!string.IsNullOrEmpty(_urlDynamicFormat))
			{
				_urlDynamicFormat = _urlDynamicFormat.Replace("{subdomain}", "t{0}").Replace("{quadkey}", "{1}");
			}
		}
	}

	private string MakeTileImageUrl(GPoint pos, int zoom, string language)
	{
		string text = TileXYToQuadKey(pos.X, pos.Y, zoom);
		if (!DisableDynamicTileUrlFormat && !string.IsNullOrEmpty(_urlDynamicFormat))
		{
			return string.Format(_urlDynamicFormat, GMapProvider.GetServerNum(pos, 4), text);
		}
		return string.Format(UrlFormat, GMapProvider.GetServerNum(pos, 4), text, Version, language, ForceSessionIdOnTileAccess ? ("&key=" + SessionId) : string.Empty);
	}
}
