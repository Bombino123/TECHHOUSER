using System;

namespace GMap.NET.MapProviders;

public class BingHybridMapProvider : BingMapProviderBase
{
	public static readonly BingHybridMapProvider Instance;

	private string _urlDynamicFormat = string.Empty;

	private static readonly string UrlFormat;

	public override Guid Id { get; } = new Guid("94E2FCB4-CAAC-45EA-A1F9-8147C4B14970");


	public override string Name { get; } = "BingHybridMap";


	private BingHybridMapProvider()
	{
	}

	static BingHybridMapProvider()
	{
		UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/h{1}.jpeg?g={2}&mkt={3}&n=z{4}";
		Instance = new BingHybridMapProvider();
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
			_urlDynamicFormat = GetTileUrl("AerialWithLabels");
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
